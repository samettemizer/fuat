using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;

/// <summary>
/// Mevcut duyurunun metnini, yayin penceresini ve aktiflik durumunu gunceller.
/// </summary>
public sealed class DuyuruGuncelleHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<DuyuruGuncelleResponse>> HandleAsync(
        Guid apartmanId,
        Guid duyuruId,
        DuyuruGuncelleRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<DuyuruGuncelleResponse>(accessResult.Error!);
        }

        var duyuru = await dbContext.Duyurular
            .FirstOrDefaultAsync(d => d.Id == duyuruId && d.ApartmanId == apartmanId, cancellationToken);

        if (duyuru is null)
        {
            return Result.Failure<DuyuruGuncelleResponse>(
                Error.NotFound("duyuru.not_found", "Duyuru kaydı bulunamadı."));
        }

        duyuru.Title = request.Title.Trim();
        duyuru.Content = request.Content.Trim();
        duyuru.PublishedOn = request.PublishedOn;
        duyuru.ExpiresOn = request.ExpiresOn;
        duyuru.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DuyuruGuncelleResponse(
            duyuru.Id,
            duyuru.ApartmanId,
            duyuru.Title,
            duyuru.Content,
            duyuru.PublishedOn,
            duyuru.ExpiresOn,
            duyuru.IsActive));
    }
}
