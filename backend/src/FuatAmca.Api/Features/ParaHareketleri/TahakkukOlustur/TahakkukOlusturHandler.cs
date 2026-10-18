using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

/// <summary>
/// Secilen donem icin apartmanin aktif uyelerine aylik gider tahakkuklarini uretir.
/// Idempotenttir: o donemde kaydi bulunan uye x kalem kombinasyonlari atlanir,
/// ayni ay ikinci kez calistirildiginda yeni kayit uretilmez.
/// </summary>
public sealed class TahakkukOlusturHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    BakiyeGuncelleyici bakiyeGuncelleyici,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<TahakkukOlusturResponse>> HandleAsync(
        Guid apartmanId,
        TahakkukOlusturRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<TahakkukOlusturResponse>(accessResult.Error!);
        }

        var apartman = await dbContext.Apartmanlar
            .AsNoTracking()
            .Where(a => a.Id == apartmanId)
            .Select(a => new
            {
                a.IsinmaTipi,
                a.SonOdemeGunu,
                Settings = a.GiderAyarlari
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.GiderTipi)
                    .Select(g => new { g.GiderTipi, g.VarsayilanTutar, g.KiracidanAlinirMi })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (apartman is null)
        {
            return Result.Failure<TahakkukOlusturResponse>(
                Error.NotFound("apartman.not_found", "Apartman kaydı bulunamadı."));
        }

        var selectedKalemler = request.GiderTipleri is { Count: > 0 }
            ? request.GiderTipleri.ToHashSet()
            : null;

        var settings = selectedKalemler is null
            ? apartman.Settings
            : apartman.Settings.Where(a => selectedKalemler.Contains(a.GiderTipi)).ToList();

        if (settings.Count == 0)
        {
            return Result.Failure<TahakkukOlusturResponse>(Error.BusinessRule(
                "tahakkuk.no_items",
                "Bu apartmanda tahakkuk üretilecek aktif gider kalemi bulunamadı."));
        }

        var members = await dbContext.Members
            .AsNoTracking()
            .Where(u => u.ApartmanId == apartmanId && u.IsActive)
            .Select(u => new { u.Id, u.SakinTipi })
            .ToListAsync(cancellationToken);

        // Donemde zaten yazilmis tahakkuklar tek sorguda cekilir; kombinasyon
        // kontrolu bellekte yapilir (uye basina sorgu yok).
        var existingRecords = await dbContext.ParaHareketleri
            .AsNoTracking()
            .Where(h => h.ApartmanId == apartmanId
                        && h.Yon == HareketYonu.Tahakkuk
                        && h.DonemYil == request.Yil
                        && h.DonemAy == request.Ay)
            .Select(h => new { h.MemberId, h.GiderTipi })
            .ToListAsync(cancellationToken);

        var existingSet = existingRecords
            .Select(k => (k.MemberId, k.GiderTipi))
            .ToHashSet();

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.GetRequiredUserId();
        var islemTarihi = new DateOnly(
            request.Yil,
            request.Ay,
            Math.Min(apartman.SonOdemeGunu, DateTime.DaysInMonth(request.Yil, request.Ay)));
        var description = request.Description?.Trim();

        // BakiyeGuncelleyici master bakiye kaydini veritabanindan okur; eksik
        // kayitlar donguden once tek seferde yazilmazsa ayni uye icin birden
        // fazla MemberBakiye satiri olusturmaya calisir.
        await EksikBakiyeKayitlariniHazirlaAsync(apartmanId, members.Select(u => u.Id), cancellationToken);

        var kalemOzetleri = new Dictionary<GiderTipi, (int RecordCount, decimal ToplamTutar)>();
        var created = 0;
        var skipped = 0;
        var toplamTutar = 0m;

        foreach (var member in members)
        {
            foreach (var setting in settings)
            {
                if (existingSet.Contains((member.Id, setting.GiderTipi)))
                {
                    skipped++;
                    continue;
                }

                var tutar = TahakkukKurallari.TutarBelirle(
                    setting.GiderTipi,
                    apartman.IsinmaTipi,
                    member.SakinTipi,
                    setting.VarsayilanTutar,
                    setting.KiracidanAlinirMi);

                if (tutar is null)
                {
                    skipped++;
                    continue;
                }

                var hareket = new ParaHareketi
                {
                    ApartmanId = apartmanId,
                    MemberId = member.Id,
                    GiderTipi = setting.GiderTipi,
                    Yon = HareketYonu.Tahakkuk,
                    Tutar = tutar.Value,
                    DonemYil = request.Yil,
                    DonemAy = request.Ay,
                    Muhlet = request.Muhlet,
                    Description = description,
                    IslemTarihi = islemTarihi,
                    RecordedAt = now,
                    CreatedByUserId = userId
                };

                dbContext.ParaHareketleri.Add(hareket);
                await bakiyeGuncelleyici.UygulaAsync(hareket, cancellationToken);

                created++;
                toplamTutar += tutar.Value;

                var summary = kalemOzetleri.GetValueOrDefault(setting.GiderTipi);
                kalemOzetleri[setting.GiderTipi] = (summary.RecordCount + 1, summary.ToplamTutar + tutar.Value);
            }
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var kalemler = kalemOzetleri
            .OrderBy(kalem => kalem.Key)
            .Select(kalem => new TahakkukKalemSummaryResponse(
                kalem.Key,
                kalem.Value.RecordCount,
                kalem.Value.ToplamTutar))
            .ToList();

        return Result.Success(new TahakkukOlusturResponse(
            request.Yil,
            request.Ay,
            members.Count,
            created,
            skipped,
            toplamTutar,
            kalemler));
    }

    /// <summary>
    /// Master bakiye satiri olmayan uyeler icin sifir bakiyeli kayitlari olusturur.
    /// </summary>
    private async Task EksikBakiyeKayitlariniHazirlaAsync(
        Guid apartmanId,
        IEnumerable<Guid> memberIds,
        CancellationToken cancellationToken)
    {
        var membersWithBakiye = await dbContext.MemberBakiyeleri
            .AsNoTracking()
            .Where(b => b.ApartmanId == apartmanId)
            .Select(b => b.MemberId)
            .ToListAsync(cancellationToken);

        var bakiyeKumesi = membersWithBakiye.ToHashSet();
        var hasMissing = false;

        foreach (var memberId in memberIds.Where(id => !bakiyeKumesi.Contains(id)))
        {
            dbContext.MemberBakiyeleri.Add(bakiyeGuncelleyici.BaslangicBakiyesiOlustur(memberId, apartmanId));
            hasMissing = true;
        }

        if (hasMissing)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
