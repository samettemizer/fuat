namespace FuatAmca.Api.Features.PanelUsers.CreateUser;

/// <summary>Olusturulan panel kullanicisinin ozeti.</summary>
public sealed record CreateUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> ApartmanIds,
    DateTimeOffset CreatedAt);
