using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.PanelUsers.ListUsers;

/// <summary>
/// Aktif kullanicinin yetkili oldugu apartmanlarda yetkisi bulunan panel kullanicilarini listeler.
/// Read-only: AsNoTracking + DTO projeksiyonu; roller tek join sorgusuyla toplanir.
/// </summary>
public sealed class ListUsersHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<PagedResult<UserRowResponse>>> HandleAsync(
        ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize) = PaginationRules.Normalize(request.Page, request.PageSize);

        List<Guid> scope;
        if (request.ApartmanId is { } apartmanId)
        {
            var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
            if (accessResult.IsFailure)
            {
                return Result.Failure<PagedResult<UserRowResponse>>(accessResult.Error!);
            }

            scope = [apartmanId];
        }
        else
        {
            scope = await accessService.GetAccessibleApartmanIds().ToListAsync(cancellationToken);
        }

        if (scope.Count == 0)
        {
            return Result.Success(PagedResult<UserRowResponse>.Empty(page, pageSize));
        }

        var query = dbContext.Users
            .AsNoTracking()
            .Where(k => k.ApartmanGrants.Any(ak => scope.Contains(ak.ApartmanId)));

        if (request.ActiveOnly == true)
        {
            query = query.Where(k => k.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(k =>
                EF.Functions.ILike(k.FullName, pattern)
                || (k.Email != null && EF.Functions.ILike(k.Email, pattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return Result.Success(PagedResult<UserRowResponse>.Empty(page, pageSize));
        }

        var rows = await query
            .OrderBy(k => k.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(k => new
            {
                k.Id,
                k.FullName,
                Email = k.Email,
                k.IsActive,
                k.CreatedAt,
                // Sadece istegi yapanin gorebildigi apartman yetkileri doner.
                Apartmanlar = k.ApartmanGrants
                    .Where(ak => scope.Contains(ak.ApartmanId))
                    .OrderBy(ak => ak.Apartman!.Name)
                    .Select(ak => new UserApartmanResponse(ak.ApartmanId, ak.Apartman!.Name))
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var userIds = rows.ConvertAll(row => row.Id);

        // Roller tek sorguda (UserRoles + Roles join) cekilir; satir basina ek sorgu yok.
        var roleRecords = await dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userIds.Contains(userRole.UserId))
            .Join(
                dbContext.Roles,
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, RoleName = role.Name! })
            .ToListAsync(cancellationToken);

        var rolesByUserId = roleRecords
            .GroupBy(record => record.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(record => record.RoleName)
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        var items = rows.ConvertAll(row => new UserRowResponse(
            row.Id,
            row.FullName,
            row.Email ?? string.Empty,
            row.IsActive,
            row.CreatedAt,
            rolesByUserId.TryGetValue(row.Id, out var roles) ? roles : Array.Empty<string>(),
            row.Apartmanlar));

        return Result.Success(new PagedResult<UserRowResponse>(
            items,
            totalCount,
            page,
            pageSize));
    }
}
