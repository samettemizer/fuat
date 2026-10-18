namespace FuatAmca.Api.Features.PanelUsers.ListUsers;

/// <summary>Panel kullanicisi listesi icin filtre ve sayfalama parametreleri.</summary>
public sealed record ListUsersRequest(
    Guid? ApartmanId,
    bool? ActiveOnly,
    string? Search,
    int? Page,
    int? PageSize);
