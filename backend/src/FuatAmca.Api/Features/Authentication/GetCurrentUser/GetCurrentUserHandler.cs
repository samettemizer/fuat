using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Authentication.GetCurrentUser;

/// <summary>
/// Oturum acmis kullanicinin profilini ve yetkili oldugu apartmanlari dondurur.
/// Read-only sorgu: AsNoTracking + dogrudan DTO projeksiyonu.
/// </summary>
public sealed class GetCurrentUserHandler(FuatAmcaDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<Result<GetCurrentUserResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(k => k.Id == userId)
            .Select(k => new { k.Id, k.FullName, k.Email })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<GetCurrentUserResponse>(
                Error.NotFound("user.not_found", "Kullanıcı kaydı bulunamadı."));
        }

        var apartmanlar = await dbContext.ApartmanUsers
            .AsNoTracking()
            .Where(ak => ak.UserId == userId)
            .OrderBy(ak => ak.Apartman!.Name)
            .Select(ak => new AuthorizedApartmanResponse(ak.ApartmanId, ak.Apartman!.Name, ak.Apartman!.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(new GetCurrentUserResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            [.. currentUser.Roles],
            apartmanlar));
    }
}
