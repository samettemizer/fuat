namespace FuatAmca.Api.Features.Authentication.Logout;

/// <summary>
/// Logout istegi. Token verilirse sadece o oturum, verilmezse kullanicinin
/// tum aktif refresh tokenlari iptal edilir.
/// </summary>
public sealed record LogoutRequest(string? RefreshToken);
