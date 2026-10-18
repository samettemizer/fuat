namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Panel kullanicisinin hangi apartmanlarda yetkili oldugunu tutan baglanti kaydi.
/// </summary>
public sealed class ApartmanUser
{
    public Guid ApartmanId { get; set; }

    public Apartman? Apartman { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
