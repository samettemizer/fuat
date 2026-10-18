using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanDetay;

/// <summary>
/// Secili apartmanin detayini ve gider ayarlarini dondurur.
/// </summary>
public sealed class ApartmanDetayHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<ApartmanDetayResponse>> HandleAsync(Guid apartmanId, CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<ApartmanDetayResponse>(accessResult.Error!);
        }

        var apartman = await dbContext.Apartmanlar
            .AsNoTracking()
            .Where(a => a.Id == apartmanId)
            .Select(a => new ApartmanDetayResponse(
                a.Id,
                a.Name,
                a.Address,
                a.IsinmaTipi,
                a.SonOdemeGunu,
                a.IsActive,
                a.CreatedAt,
                a.Members.Count(u => u.IsActive),
                a.Members.Where(u => u.Bakiye != null && u.Bakiye.Bakiye > 0m).Sum(u => u.Bakiye!.Bakiye),
                a.GiderAyarlari
                    .OrderBy(g => g.GiderTipi)
                    .Select(g => new ApartmanGiderAyariResponse(
                        g.GiderTipi,
                        g.VarsayilanTutar,
                        g.KiracidanAlinirMi,
                        g.IsActive))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return apartman is null
            ? Result.Failure<ApartmanDetayResponse>(
                Error.NotFound("apartman.not_found", "Apartman kaydı bulunamadı."))
            : Result.Success(apartman);
    }
}
