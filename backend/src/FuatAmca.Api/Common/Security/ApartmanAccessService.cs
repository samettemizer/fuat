using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Common.Security;

/// <summary>
/// Panel kullanicisinin ilgili apartmana yetkili olup olmadigini kontrol eder.
/// Tum apartman kapsamli handler'lar isleme baslamadan bunu cagirir.
/// </summary>
public sealed class ApartmanAccessService(FuatAmcaDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<bool> HasAccessAsync(Guid apartmanId, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
        {
            return false;
        }

        return await dbContext.ApartmanUsers
            .AnyAsync(ak => ak.ApartmanId == apartmanId && ak.UserId == userId.Value, cancellationToken);
    }

    /// <summary>Erisim yoksa Erisim turunde hata dondurur.</summary>
    public async Task<Result> CheckAccessAsync(Guid apartmanId, CancellationToken cancellationToken) =>
        await HasAccessAsync(apartmanId, cancellationToken)
            ? Result.Success()
            : Result.Failure(Error.Forbidden("apartman.access_denied", "Bu apartman icin yetkiniz bulunmuyor."));

    /// <summary>Aktif kullanicinin yetkili oldugu apartman kimlikleri.</summary>
    public IQueryable<Guid> GetAccessibleApartmanIds()
    {
        var userId = currentUser.UserId ?? Guid.Empty;
        return dbContext.ApartmanUsers
            .Where(ak => ak.UserId == userId)
            .Select(ak => ak.ApartmanId);
    }
}
