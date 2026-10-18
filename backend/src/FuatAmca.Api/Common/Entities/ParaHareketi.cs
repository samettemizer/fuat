using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Uyenin tek bir para hareketi: ya tahakkuk (borc yazma) ya da odeme.
/// Tum bakiye hesaplari bu defterden uretilir, master tablolar bunun ozetidir.
/// </summary>
public sealed class ParaHareketi
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ApartmanId { get; set; }

    public Apartman? Apartman { get; set; }

    public Guid MemberId { get; set; }

    public Member? Member { get; set; }

    public GiderTipi GiderTipi { get; set; }

    public HareketYonu Yon { get; set; }

    /// <summary>Hareket tutari. Odemede uye, borcundan daha az da odeyebilir.</summary>
    public decimal Tutar { get; set; }

    /// <summary>Hareketin ait oldugu donem yili (tahakkuklarda zorunlu).</summary>
    public int? DonemYil { get; set; }

    /// <summary>Hareketin ait oldugu donem ayi 1-12 (tahakkuklarda zorunlu).</summary>
    public int? DonemAy { get; set; }

    /// <summary>
    /// Muhlet: uyenin odeme gununu gecirdigi durumda taahhut ettigi odeme tarihi.
    /// Zorunlu degil; tarihi gelmis ve kapanmamis muhletler panelde toast olarak uyarir.
    /// </summary>
    public DateOnly? Muhlet { get; set; }

    public string? Description { get; set; }

    /// <summary>Hareketin gerceklestigi tarih.</summary>
    public DateOnly IslemTarihi { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Hareketi kaydeden panel kullanicisi (entegrasyon kaynakli kayitlarda bos).</summary>
    public Guid? CreatedByUserId { get; set; }
}
