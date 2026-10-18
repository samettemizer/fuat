using Microsoft.AspNetCore.Identity;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Panele giris yapan kullanici (yonetici, yardimci, denetci, sayman).
/// </summary>
public sealed class User : IdentityUser<Guid>
{
    /// <summary>Kullanicinin gorunen ad soyad bilgisi.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Pasif kullanici giris yapamaz.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Kullaniciyi panele ekleyen yonetici.</summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>Kullanicinin yetkili oldugu apartmanlar.</summary>
    public ICollection<ApartmanUser> ApartmanGrants { get; set; } = [];

    /// <summary>Aktif / gecmis refresh tokenlari.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
