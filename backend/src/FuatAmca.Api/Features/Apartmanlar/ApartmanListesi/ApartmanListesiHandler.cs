using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanListesi;

/// <summary>
/// Aktif kullanicinin yetkili oldugu apartmanlari ozet gostergelerle listeler.
/// Read-only: AsNoTracking + dogrudan DTO projeksiyonu.
/// </summary>
public sealed class ApartmanListesiHandler(FuatAmcaDbContext dbContext, ICurrentUser currentUser)
{
    public async Task<Result<IReadOnlyList<ApartmanRowResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();

        var items = await dbContext.ApartmanUsers
            .AsNoTracking()
            .Where(ak => ak.UserId == userId)
            .OrderBy(ak => ak.Apartman!.Name)
            .Select(ak => new ApartmanRowResponse(
                ak.Apartman!.Id,
                ak.Apartman.Name,
                ak.Apartman.Address,
                ak.Apartman.IsinmaTipi,
                ak.Apartman.SonOdemeGunu,
                ak.Apartman.IsActive,
                ak.Apartman.Members.Count(u => u.IsActive),
                ak.Apartman.Members
                    .Where(u => u.Bakiye != null && u.Bakiye.Bakiye > 0m)
                    .Sum(u => u.Bakiye!.Bakiye),
                -ak.Apartman.Members
                    .Where(u => u.Bakiye != null && u.Bakiye.Bakiye < 0m)
                    .Sum(u => u.Bakiye!.Bakiye)))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ApartmanRowResponse>>(items);
    }
}
