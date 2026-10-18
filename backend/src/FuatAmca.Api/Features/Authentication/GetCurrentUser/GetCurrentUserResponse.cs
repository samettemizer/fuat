namespace FuatAmca.Api.Features.Authentication.GetCurrentUser;

/// <summary>Kullanicinin yetkili oldugu apartman.</summary>
public sealed record AuthorizedApartmanResponse(Guid Id, string Name, bool IsActive);

/// <summary>Oturum acmis kullanicinin profili.</summary>
public sealed record GetCurrentUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<AuthorizedApartmanResponse> Apartmanlar);
