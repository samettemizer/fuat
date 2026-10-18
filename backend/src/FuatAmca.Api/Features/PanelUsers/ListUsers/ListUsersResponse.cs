namespace FuatAmca.Api.Features.PanelUsers.ListUsers;

/// <summary>Kullanicinin yetkili oldugu apartmanin kisa gosterimi.</summary>
public sealed record UserApartmanResponse(Guid ApartmanId, string Name);

/// <summary>Panel kullanicisi listesi satiri.</summary>
public sealed record UserRowResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<UserApartmanResponse> Apartmanlar);
