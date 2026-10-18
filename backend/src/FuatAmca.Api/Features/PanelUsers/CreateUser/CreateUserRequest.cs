namespace FuatAmca.Api.Features.PanelUsers.CreateUser;

/// <summary>Yeni panel kullanicisi olusturma istegi.</summary>
public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid> ApartmanIds);
