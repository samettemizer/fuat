namespace FuatAmca.Api.Common.Security;

/// <summary>
/// JWT yapilandirmasi. Imza anahtarlari konfigurasyondan / user-secrets'tan gelir,
/// kod icinde sabit anahtar tutulmaz.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "fuat-api";

    public string Audience { get; set; } = "fuat-panel";

    /// <summary>Erisim tokeni omru (dakika). Kisa tutulur, yenileme refresh token ile yapilir.</summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>Refresh token omru (gun).</summary>
    public int RefreshTokenLifetimeDays { get; set; } = 14;

    /// <summary>RS256 ozel anahtari (PEM icerigi). Uretimde user-secrets / env ile verilir.</summary>
    public string? PrivateKeyPem { get; set; }

    /// <summary>Sadece dogrulama yapan dagitimlar icin acik anahtar (PEM icerigi).</summary>
    public string? PublicKeyPem { get; set; }

    /// <summary>
    /// PEM icerigi verilmediyse anahtarin okunacagi dosya yolu.
    /// Development ortaminda dosya yoksa uretilip bu yola yazilir.
    /// </summary>
    public string? KeyFilePath { get; set; }
}
