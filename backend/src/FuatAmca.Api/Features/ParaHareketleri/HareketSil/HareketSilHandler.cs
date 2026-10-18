using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketSil;

/// <summary>
/// Yanlis girilen bir para hareketini siler ve uyenin bakiyesini hareket
/// defterinden bastan hesaplar.
/// </summary>
public sealed class HareketSilHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    BakiyeGuncelleyici bakiyeGuncelleyici)
{
    public async Task<Result<HareketSilResponse>> HandleAsync(
        Guid apartmanId,
        Guid hareketId,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<HareketSilResponse>(accessResult.Error!);
        }

        var hareket = await dbContext.ParaHareketleri
            .FirstOrDefaultAsync(h => h.Id == hareketId && h.ApartmanId == apartmanId, cancellationToken);

        if (hareket is null)
        {
            return Result.Failure<HareketSilResponse>(
                Error.NotFound("hareket.not_found", "Para hareketi bu apartmanda bulunamadı."));
        }

        var memberId = hareket.MemberId;

        // Yeniden hesaplama hareket defterini veritabanindan okudugu icin silme
        // once yazilir; ikisi tek transaction icinde commit edilir.
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.ParaHareketleri.Remove(hareket);
        await dbContext.SaveChangesAsync(cancellationToken);

        await bakiyeGuncelleyici.YenidenHesaplaAsync(memberId, apartmanId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var yeniBakiye = await dbContext.MemberBakiyeleri
            .AsNoTracking()
            .Where(b => b.MemberId == memberId)
            .Select(b => b.Bakiye)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new HareketSilResponse(
            hareketId,
            memberId,
            yeniBakiye,
            BakiyeHesaplayici.Durum(yeniBakiye)));
    }
}
