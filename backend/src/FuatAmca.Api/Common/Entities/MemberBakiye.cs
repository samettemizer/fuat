namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Uyenin master bakiye tablosu. Her para hareketi girisinde ayni transaction
/// icinde guncellenir; uye listesi bu tabloyu okur, hareketleri toplamaz.
/// Bakiye pozitifse uye borclu, negatifse alacaklidir.
/// </summary>
public sealed class MemberBakiye
{
    public Guid MemberId { get; set; }

    public Member? Member { get; set; }

    public Guid ApartmanId { get; set; }

    /// <summary>Uyeye yazilan toplam borc.</summary>
    public decimal TahakkukToplam { get; set; }

    /// <summary>Uyenin yaptigi toplam odeme.</summary>
    public decimal OdemeToplam { get; set; }

    /// <summary>TahakkukToplam - OdemeToplam. Pozitif = borc, negatif = alacak.</summary>
    public decimal Bakiye { get; set; }

    /// <summary>Vadesi gecmis ve henuz kapanmamis en yakin muhlet tarihi.</summary>
    public DateOnly? EnYakinMuhlet { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
