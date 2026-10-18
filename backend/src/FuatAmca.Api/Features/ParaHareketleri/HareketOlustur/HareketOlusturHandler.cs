using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;

/// <summary>
/// Elle tek para hareketi (tahakkuk veya odeme) kaydeder ve uyenin master
/// bakiye tablolarini ayni transaction icinde gunceller.
/// </summary>
public sealed class HareketOlusturHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    BakiyeGuncelleyici bakiyeGuncelleyici,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<HareketOlusturResponse>> HandleAsync(
        Guid apartmanId,
        HareketOlusturRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<HareketOlusturResponse>(accessResult.Error!);
        }

        var member = await dbContext.Members
            .AsNoTracking()
            .Where(u => u.Id == request.MemberId && u.ApartmanId == apartmanId)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.DaireNo })
            .FirstOrDefaultAsync(cancellationToken);

        if (member is null)
        {
            return Result.Failure<HareketOlusturResponse>(
                Error.NotFound("member.not_found", "Üye kaydı bu apartmanda bulunamadı."));
        }

        // Ayni uyeye ayni donem / gider kalemi icin ikinci kez tahakkuk yazilamaz.
        // Veritabanindaki tekil indeksin ihlalini beklemeden anlasilir hata dondurulur.
        if (request.Yon == HareketYonu.Tahakkuk && request.DonemYil is not null && request.DonemAy is not null)
        {
            var tahakkukVarMi = await dbContext.ParaHareketleri
                .AnyAsync(
                    h => h.MemberId == request.MemberId
                         && h.GiderTipi == request.GiderTipi
                         && h.Yon == HareketYonu.Tahakkuk
                         && h.DonemYil == request.DonemYil
                         && h.DonemAy == request.DonemAy,
                    cancellationToken);

            if (tahakkukVarMi)
            {
                return Result.Failure<HareketOlusturResponse>(Error.Conflict(
                    "hareket.tahakkuk_exists",
                    $"Bu üyeye {TahakkukKurallari.DonemEtiketi(request.DonemYil, request.DonemAy)} dönemi için "
                    + "aynı gider kaleminden tahakkuk zaten yazılmış."));
            }
        }

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var hareket = new ParaHareketi
        {
            ApartmanId = apartmanId,
            MemberId = member.Id,
            GiderTipi = request.GiderTipi,
            Yon = request.Yon,
            Tutar = decimal.Round(request.Tutar, 2, MidpointRounding.AwayFromZero),
            DonemYil = request.DonemYil,
            DonemAy = request.DonemAy,
            Muhlet = request.Muhlet,
            Description = request.Description?.Trim(),
            IslemTarihi = request.IslemTarihi ?? today,
            RecordedAt = now,
            CreatedByUserId = currentUser.GetRequiredUserId()
        };

        dbContext.ParaHareketleri.Add(hareket);
        await bakiyeGuncelleyici.UygulaAsync(hareket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var yeniBakiye = await dbContext.MemberBakiyeleri
            .AsNoTracking()
            .Where(b => b.MemberId == member.Id)
            .Select(b => b.Bakiye)
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new HareketOlusturResponse(
            hareket.Id,
            hareket.MemberId,
            $"{member.FirstName} {member.LastName}",
            member.DaireNo,
            hareket.GiderTipi,
            hareket.Yon,
            hareket.Tutar,
            hareket.IslemTarihi,
            hareket.Muhlet,
            yeniBakiye,
            BakiyeHesaplayici.Durum(yeniBakiye),
            BakiyeHesaplayici.MuhletGectiMi(hareket.Muhlet, yeniBakiye, today)));
    }
}
