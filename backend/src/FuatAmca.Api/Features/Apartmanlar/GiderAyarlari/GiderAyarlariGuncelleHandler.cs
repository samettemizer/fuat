using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

/// <summary>
/// Aylik gider kalemi ayarlarini guncellemek. Bireysel kombili binada yakit
/// tutari zorla 0 TL tutulur; bina tadilati kiracidan alinmaz.
/// </summary>
public sealed class GiderAyarlariGuncelleHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<GiderAyarlariResponse>> HandleAsync(
        Guid apartmanId,
        GiderAyarlariGuncelleRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<GiderAyarlariResponse>(accessResult.Error!);
        }

        var apartman = await dbContext.Apartmanlar
            .Include(a => a.GiderAyarlari)
            .FirstOrDefaultAsync(a => a.Id == apartmanId, cancellationToken);

        if (apartman is null)
        {
            return Result.Failure<GiderAyarlariResponse>(
                Error.NotFound("apartman.not_found", "Apartman kaydı bulunamadı."));
        }

        var now = timeProvider.GetUtcNow();

        foreach (var incoming in request.Kalemler)
        {
            var tutar = incoming.GiderTipi == GiderTipi.Yakit && apartman.IsinmaTipi == IsinmaTipi.BireyselKombi
                ? 0m
                : decimal.Round(incoming.VarsayilanTutar, 2, MidpointRounding.AwayFromZero);

            // Bina tadilati her kosulda kiraciya yazilmaz.
            var kiracidanAlinirMi = incoming.GiderTipi != GiderTipi.BinaTadilati && incoming.KiracidanAlinirMi;

            var kalem = apartman.GiderAyarlari.FirstOrDefault(g => g.GiderTipi == incoming.GiderTipi);
            if (kalem is null)
            {
                apartman.GiderAyarlari.Add(new ApartmanGiderAyari
                {
                    ApartmanId = apartmanId,
                    GiderTipi = incoming.GiderTipi,
                    VarsayilanTutar = tutar,
                    KiracidanAlinirMi = kiracidanAlinirMi,
                    IsActive = incoming.IsActive,
                    UpdatedAt = now
                });
                continue;
            }

            kalem.VarsayilanTutar = tutar;
            kalem.KiracidanAlinirMi = kiracidanAlinirMi;
            kalem.IsActive = incoming.IsActive;
            kalem.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new GiderAyarlariResponse(
            apartman.Id,
            apartman.IsinmaTipi,
            [.. apartman.GiderAyarlari
                .OrderBy(g => g.GiderTipi)
                .Select(g => new GiderKalemiResponse(
                    g.GiderTipi,
                    g.VarsayilanTutar,
                    g.KiracidanAlinirMi,
                    g.IsActive,
                    g.UpdatedAt))]));
    }
}
