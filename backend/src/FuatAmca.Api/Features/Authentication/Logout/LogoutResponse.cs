namespace FuatAmca.Api.Features.Authentication.Logout;

/// <summary>Iptal edilen token sayisi.</summary>
public sealed record LogoutResponse(int RevokedTokenCount);
