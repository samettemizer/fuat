namespace FuatAmca.Api.Features.PanelUsers.UpdateUser;

/// <summary>Panel kullanicisi guncelleme istegi. Roller ve apartman yetkileri senkronize edilir.</summary>
public sealed record UpdateUserRequest(
    string FullName,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> ApartmanIds);
