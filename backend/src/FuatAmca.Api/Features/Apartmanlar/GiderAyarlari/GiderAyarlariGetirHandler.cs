using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

/// <summary>
/// Apartmanin aylik gider kalemi ayarlarini dondurur.
/// </summary>
public sealed class GiderAyarlariGetirHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<GiderAyarlariResponse>> HandleAsync(Guid apartmanId, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<GiderAyarlariResponse>(accessResult.Error!);
        }

        var settings = await dbContext.Apartmanlar
            .AsNoTracking()
            .Where(a => a.Id == apartmanId)
            .Select(a => new GiderAyarlariResponse(
                a.Id,
                a.IsinmaTipi,
                a.GiderAyarlari
                    .OrderBy(g => g.GiderTipi)
                    .Select(g => new GiderKalemiResponse(
                        g.GiderTipi,
                        g.VarsayilanTutar,
                        g.KiracidanAlinirMi,
                        g.IsActive,
                        g.UpdatedAt))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return settings is null
            ? Result.Failure<GiderAyarlariResponse>(
                Error.NotFound("apartman.not_found", "Apartman kaydı bulunamadı."))
            : Result.Success(settings);
    }
}
