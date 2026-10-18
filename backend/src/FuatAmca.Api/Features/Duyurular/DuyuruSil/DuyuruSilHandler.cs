using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Duyurular.DuyuruSil;

/// <summary>
/// Duyuru kaydini kalici olarak siler (soft delete uygulanmaz).
/// </summary>
public sealed class DuyuruSilHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<DuyuruSilResponse>> HandleAsync(
        Guid apartmanId,
        Guid duyuruId,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<DuyuruSilResponse>(accessResult.Error!);
        }

        var duyuru = await dbContext.Duyurular
            .FirstOrDefaultAsync(d => d.Id == duyuruId && d.ApartmanId == apartmanId, cancellationToken);

        if (duyuru is null)
        {
            return Result.Failure<DuyuruSilResponse>(
                Error.NotFound("duyuru.not_found", "Duyuru kaydı bulunamadı."));
        }

        dbContext.Duyurular.Remove(duyuru);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DuyuruSilResponse(duyuruId));
    }
}
