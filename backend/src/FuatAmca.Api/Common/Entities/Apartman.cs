using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Yonetilen apartman / site kaydi. Bir yonetici birden fazla apartman yonetebilir.
/// </summary>
public sealed class Apartman
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    /// <summary>Binanin isinma sekli; yakit tahakkukunu belirler.</summary>
    public IsinmaTipi IsinmaTipi { get; set; } = IsinmaTipi.MerkeziSistem;

    /// <summary>Aylik giderlerin son odeme gunu (1-28).</summary>
    public int SonOdemeGunu { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Apartmani olusturan panel kullanicisi.</summary>
    public Guid CreatedByUserId { get; set; }

    public ICollection<ApartmanGiderAyari> GiderAyarlari { get; set; } = [];

    public ICollection<ApartmanUser> Users { get; set; } = [];

    public ICollection<Member> Members { get; set; } = [];

    public ICollection<Duyuru> Duyurular { get; set; } = [];
}
