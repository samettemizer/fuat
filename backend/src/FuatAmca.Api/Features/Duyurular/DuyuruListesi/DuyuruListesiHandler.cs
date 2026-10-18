using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Duyurular.DuyuruListesi;

/// <summary>
/// Apartmanin duyurularini sayfali listeler. Read-only: AsNoTracking + DTO projeksiyonu.
/// </summary>
public sealed class DuyuruListesiHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<PagedResult<DuyuruRowResponse>>> HandleAsync(
        Guid apartmanId,
        DuyuruListesiRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<PagedResult<DuyuruRowResponse>>(accessResult.Error!);
        }

        var (page, pageSize) = PaginationRules.Normalize(request.Page, request.PageSize);

        var query = dbContext.Duyurular
            .AsNoTracking()
            .Where(d => d.ApartmanId == apartmanId);

        if (request.ActiveOnly == true)
        {
            // Su an yayinda olan duyurular: aktif isaretli ve tarih penceresi icinde.
            var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            query = query.Where(d =>
                d.IsActive && d.PublishedOn <= today && (d.ExpiresOn == null || d.ExpiresOn >= today));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.PublishedOn)
            .ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DuyuruRowResponse(
                d.Id,
                d.ApartmanId,
                d.Title,
                d.Content,
                d.PublishedOn,
                d.ExpiresOn,
                d.IsActive,
                d.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<DuyuruRowResponse>(
            items,
            totalCount,
            page,
            pageSize));
    }
}
