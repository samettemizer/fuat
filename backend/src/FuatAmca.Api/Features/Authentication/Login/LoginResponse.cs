namespace FuatAmca.Api.Features.Authentication.Login;

/// <summary>Login yanitindaki apartman ozeti; panelde "Secili Apartman" icin kullanilir.</summary>
public sealed record ApartmanOptionResponse(Guid Id, string Name);

/// <summary>Login yaniti: token cifti ve kullanici ozeti.</summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<ApartmanOptionResponse> Apartmanlar);
