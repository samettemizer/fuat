namespace FuatAmca.Api.Features.Authentication.RefreshToken;

/// <summary>Rotasyon sonrasi uretilen yeni token cifti.</summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
