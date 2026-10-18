using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;
using System.IdentityModel.Tokens.Jwt;

namespace FuatAmca.Api.Common.Security;

/// <summary>
/// Uretilen token cifti.
/// </summary>
public sealed record TokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

/// <summary>
/// Kisa omurlu RS256 erisim tokeni ve rotasyona giren refresh token uretir.
/// Refresh token'in kendisi degil SHA-256 ozeti saklanir.
/// </summary>
public sealed class TokenGenerator(
    FuatAmcaDbContext dbContext,
    IOptions<JwtSettings> settings,
    SigningKeyProvider signingKeyProvider,
    TimeProvider timeProvider)
{
    private readonly JwtSettings _settings = settings.Value;

    /// <summary>
    /// Kullanici icin yeni token cifti uretir ve refresh token kaydini ekler.
    /// Kayit SaveChanges'i cagiran tarafa (handler) birakilir.
    /// </summary>
    public TokenPair Generate(
        User user,
        IReadOnlyCollection<string> roles,
        string? clientIp,
        Guid? previousTokenId = null)
    {
        var now = timeProvider.GetUtcNow();
        var accessTokenExpiresAt = now.AddMinutes(_settings.AccessTokenLifetimeMinutes);
        var refreshTokenExpiresAt = now.AddDays(_settings.RefreshTokenLifetimeDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(FuatAmcaClaims.FullName, user.FullName)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = accessTokenExpiresAt.UtcDateTime,
            SigningCredentials = signingKeyProvider.SigningCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler { SetDefaultTimesOnTokenCreation = false };
        var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        var refreshToken = GenerateRefreshToken();
        var record = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = now,
            ClientIp = clientIp
        };

        dbContext.RefreshTokens.Add(record);

        if (previousTokenId is not null)
        {
            // Rotasyon: kullanilan token'in yerine gelen kayit isaretlenir.
            var previousRecord = dbContext.ChangeTracker.Entries<RefreshToken>()
                .Select(e => e.Entity)
                .FirstOrDefault(t => t.Id == previousTokenId.Value);

            if (previousRecord is not null)
            {
                previousRecord.ReplacedByTokenId = record.Id;
            }
        }

        return new TokenPair(accessToken, accessTokenExpiresAt, refreshToken, refreshTokenExpiresAt);
    }

    /// <summary>Verilen refresh token'i ozetiyle bulur (kullanici ile birlikte).</summary>
    public Task<RefreshToken?> FindRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);
        return dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
    }

    /// <summary>Refresh token'in SHA-256 ozetini (base64) dondurur.</summary>
    public static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string GenerateRefreshToken()
    {
        var buffer = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

/// <summary>
/// JWT standart talep adlari (System.IdentityModel sabitleri yerine sade kullanim).
/// </summary>
internal static class JwtRegisteredClaimNames
{
    public const string Sub = "sub";
    public const string Jti = "jti";
}
