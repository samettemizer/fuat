using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Apartmanda oturan kisi (uye). Kaydi sadece panel kullanicilari olusturur,
/// uyelerin kendi kendine kayit olabilecegi bir akis yoktur.
/// </summary>
public sealed class Member
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ApartmanId { get; set; }

    public Apartman? Apartman { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>Daire no; harf icerebilir (ornek: "12", "3B").</summary>
    public string DaireNo { get; set; } = string.Empty;

    /// <summary>Cep telefonu: basinda sifir olmadan 10 hane.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    public SakinTipi SakinTipi { get; set; }

    /// <summary>Uye kiraci ise zorunlu: ev sahibinin ad soyadi.</summary>
    public string? EvSahibiFullName { get; set; }

    /// <summary>Ev sahibinin cep telefonu (opsiyonel, 10 hane).</summary>
    public string? EvSahibiPhoneNumber { get; set; }

    /// <summary>Kisa vadede kullanilmiyor; uye self-servis girisi icin saklaniyor.</summary>
    public string? Email { get; set; }

    /// <summary>Kisa vadede kullanilmiyor; uye self-servis girisi icin saklaniyor.</summary>
    public string? PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Uyenin master bakiye kaydi; her para hareketinde guncellenir.</summary>
    public MemberBakiye? Bakiye { get; set; }

    public ICollection<MemberBakiyeKalemi> BakiyeKalemleri { get; set; } = [];

    public ICollection<ParaHareketi> ParaHareketleri { get; set; } = [];
}
