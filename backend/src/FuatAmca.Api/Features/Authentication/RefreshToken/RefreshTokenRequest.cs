namespace FuatAmca.Api.Features.Authentication.RefreshToken;

/// <summary>Refresh token ile yeni token cifti istegi.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);
