using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;

/// <summary>
/// Apartman genel ayarlarini guncellemek. Isinma tipi bireysel kombiye cevrilirse
/// yakit kaleminin varsayilan tutari 0 TL'ye cekilir.
/// </summary>
public sealed class ApartmanGuncelleHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<ApartmanGuncelleResponse>> HandleAsync(
        Guid apartmanId,
        ApartmanGuncelleRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<ApartmanGuncelleResponse>(accessResult.Error!);
        }

        var apartman = await dbContext.Apartmanlar
            .Include(a => a.GiderAyarlari)
            .FirstOrDefaultAsync(a => a.Id == apartmanId, cancellationToken);

        if (apartman is null)
        {
            return Result.Failure<ApartmanGuncelleResponse>(
                Error.NotFound("apartman.not_found", "Apartman kaydı bulunamadı."));
        }

        apartman.Name = request.Name.Trim();
        apartman.Address = request.Address?.Trim();
        apartman.IsinmaTipi = request.IsinmaTipi;
        apartman.SonOdemeGunu = request.SonOdemeGunu;
        apartman.IsActive = request.IsActive;

        var yakitSifirlandi = false;
        if (request.IsinmaTipi == IsinmaTipi.BireyselKombi)
        {
            var yakitAyari = apartman.GiderAyarlari.FirstOrDefault(g => g.GiderTipi == GiderTipi.Yakit);
            if (yakitAyari is not null && yakitAyari.VarsayilanTutar != 0m)
            {
                yakitAyari.VarsayilanTutar = 0m;
                yakitAyari.UpdatedAt = timeProvider.GetUtcNow();
                yakitSifirlandi = true;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApartmanGuncelleResponse(
            apartman.Id,
            apartman.Name,
            apartman.IsinmaTipi,
            apartman.SonOdemeGunu,
            apartman.IsActive,
            yakitSifirlandi));
    }
}
