namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Refresh token kaydi. Token'in kendisi degil SHA-256 ozeti saklanir,
/// her kullanimda rotasyona girer (kullanilan token iptal edilir).
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public User? User { get; set; }

    /// <summary>Token'in SHA-256 ozeti (base64).</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Token kullanildiysa kullanim zamani.</summary>
    public DateTimeOffset? UsedAt { get; set; }

    /// <summary>Token iptal edildiyse iptal zamani (cikis veya rotasyon).</summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>Rotasyonda bu token'in yerine uretilen token.</summary>
    public Guid? ReplacedByTokenId { get; set; }

    public string? ClientIp { get; set; }

    /// <summary>Token su an kullanilabilir mi.</summary>
    public bool IsActive(DateTimeOffset now) =>
        UsedAt is null && RevokedAt is null && ExpiresAt > now;
}
