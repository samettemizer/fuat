namespace FuatAmca.Api.Features.PanelUsers.UpdateUser;

/// <summary>Guncelleme sonrasi kullanicinin son durumu.</summary>
public sealed record UpdateUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> ApartmanIds,
    int RevokedTokenCount);
