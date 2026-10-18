using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Common.BusinessRules;

/// <summary>
/// Para hareketi girildiginde uyenin master bakiye tablolarini ayni transaction
/// icinde guncel tutar. SaveChanges cagrisi handler'a birakilir.
/// </summary>
public sealed class BakiyeGuncelleyici(FuatAmcaDbContext dbContext, TimeProvider timeProvider)
{
    /// <summary>
    /// Tek bir hareketin master tablolara yansitilmasi.
    /// Bakiye kayitlari yoksa olusturulur.
    /// </summary>
    public async Task UygulaAsync(ParaHareketi hareket, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var (tahakkukEtkisi, odemeEtkisi) = BakiyeHesaplayici.Etki(hareket.Yon, hareket.Tutar);

        var bakiye = await BakiyeKaydiGetirAsync(hareket.MemberId, hareket.ApartmanId, cancellationToken);
        bakiye.TahakkukToplam += tahakkukEtkisi;
        bakiye.OdemeToplam += odemeEtkisi;
        bakiye.Bakiye = BakiyeHesaplayici.Bakiye(bakiye.TahakkukToplam, bakiye.OdemeToplam);
        bakiye.UpdatedAt = now;

        var kalem = await KalemGetirAsync(hareket.MemberId, hareket.ApartmanId, hareket.GiderTipi, cancellationToken);
        kalem.TahakkukToplam += tahakkukEtkisi;
        kalem.OdemeToplam += odemeEtkisi;
        kalem.Bakiye = BakiyeHesaplayici.Bakiye(kalem.TahakkukToplam, kalem.OdemeToplam);
        kalem.UpdatedAt = now;

        // Muhlet takibi: yeni muhlet geldiyse veya borc kapandiysa en yakin muhlet tazelenir.
        if (hareket.Muhlet is not null || (bakiye.Bakiye <= 0m && bakiye.EnYakinMuhlet is not null))
        {
            bakiye.EnYakinMuhlet = await EnYakinMuhletHesaplaAsync(hareket.MemberId, bakiye.Bakiye, cancellationToken);
        }
    }

    /// <summary>
    /// Uyenin bakiyesini hareket defterinden bastan hesaplar (hareket silme / duzeltme sonrasi).
    /// </summary>
    public async Task YenidenHesaplaAsync(Guid memberId, Guid apartmanId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        var kalemToplamlari = await dbContext.ParaHareketleri
            .AsNoTracking()
            .Where(h => h.MemberId == memberId)
            .GroupBy(h => h.GiderTipi)
            .Select(g => new
            {
                GiderTipi = g.Key,
                Tahakkuk = g.Where(h => h.Yon == HareketYonu.Tahakkuk).Sum(h => (decimal?)h.Tutar) ?? 0m,
                Odeme = g.Where(h => h.Yon == HareketYonu.Odeme).Sum(h => (decimal?)h.Tutar) ?? 0m
            })
            .ToListAsync(cancellationToken);

        var bakiye = await BakiyeKaydiGetirAsync(memberId, apartmanId, cancellationToken);
        bakiye.TahakkukToplam = kalemToplamlari.Sum(k => k.Tahakkuk);
        bakiye.OdemeToplam = kalemToplamlari.Sum(k => k.Odeme);
        bakiye.Bakiye = BakiyeHesaplayici.Bakiye(bakiye.TahakkukToplam, bakiye.OdemeToplam);
        bakiye.UpdatedAt = now;
        bakiye.EnYakinMuhlet = await EnYakinMuhletHesaplaAsync(memberId, bakiye.Bakiye, cancellationToken);

        var mevcutKalemler = await dbContext.MemberBakiyeKalemleri
            .Where(k => k.MemberId == memberId)
            .ToListAsync(cancellationToken);

        foreach (var kalem in mevcutKalemler)
        {
            var toplam = kalemToplamlari.FirstOrDefault(k => k.GiderTipi == kalem.GiderTipi);
            kalem.TahakkukToplam = toplam?.Tahakkuk ?? 0m;
            kalem.OdemeToplam = toplam?.Odeme ?? 0m;
            kalem.Bakiye = BakiyeHesaplayici.Bakiye(kalem.TahakkukToplam, kalem.OdemeToplam);
            kalem.UpdatedAt = now;
        }

        foreach (var toplam in kalemToplamlari.Where(t => mevcutKalemler.TrueForAll(k => k.GiderTipi != t.GiderTipi)))
        {
            dbContext.MemberBakiyeKalemleri.Add(new MemberBakiyeKalemi
            {
                MemberId = memberId,
                ApartmanId = apartmanId,
                GiderTipi = toplam.GiderTipi,
                TahakkukToplam = toplam.Tahakkuk,
                OdemeToplam = toplam.Odeme,
                Bakiye = BakiyeHesaplayici.Bakiye(toplam.Tahakkuk, toplam.Odeme),
                UpdatedAt = now
            });
        }
    }

    /// <summary>Uye olusturuldugunda bos master bakiye kaydini hazirlar.</summary>
    public MemberBakiye BaslangicBakiyesiOlustur(Guid memberId, Guid apartmanId) => new()
    {
        MemberId = memberId,
        ApartmanId = apartmanId,
        TahakkukToplam = 0m,
        OdemeToplam = 0m,
        Bakiye = 0m,
        UpdatedAt = timeProvider.GetUtcNow()
    };

    private async Task<MemberBakiye> BakiyeKaydiGetirAsync(Guid memberId, Guid apartmanId, CancellationToken cancellationToken)
    {
        var bakiye = await dbContext.MemberBakiyeleri.FirstOrDefaultAsync(b => b.MemberId == memberId, cancellationToken);
        if (bakiye is not null)
        {
            return bakiye;
        }

        bakiye = BaslangicBakiyesiOlustur(memberId, apartmanId);
        dbContext.MemberBakiyeleri.Add(bakiye);
        return bakiye;
    }

    private async Task<MemberBakiyeKalemi> KalemGetirAsync(
        Guid memberId,
        Guid apartmanId,
        GiderTipi giderTipi,
        CancellationToken cancellationToken)
    {
        var kalem = await dbContext.MemberBakiyeKalemleri
            .FirstOrDefaultAsync(k => k.MemberId == memberId && k.GiderTipi == giderTipi, cancellationToken);

        if (kalem is not null)
        {
            return kalem;
        }

        kalem = new MemberBakiyeKalemi
        {
            MemberId = memberId,
            ApartmanId = apartmanId,
            GiderTipi = giderTipi,
            UpdatedAt = timeProvider.GetUtcNow()
        };

        dbContext.MemberBakiyeKalemleri.Add(kalem);
        return kalem;
    }

    /// <summary>Uye halen borcluysa acik en yakin muhlet tarihini bulur.</summary>
    private async Task<DateOnly?> EnYakinMuhletHesaplaAsync(Guid memberId, decimal bakiye, CancellationToken cancellationToken)
    {
        if (bakiye <= 0m)
        {
            return null;
        }

        return await dbContext.ParaHareketleri
            .AsNoTracking()
            .Where(h => h.MemberId == memberId && h.Muhlet != null)
            .OrderBy(h => h.Muhlet)
            .Select(h => h.Muhlet)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
