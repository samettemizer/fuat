using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.ListMembers;

/// <summary>
/// Uye listesi sorgu parametreleri (query string). Endpointte [AsParameters] ile baglanir.
/// Siralama: "daireNo" (varsayilan), "fullName", "bakiye", "createdAt"; SortDirection: "asc" / "desc".
/// </summary>
public sealed record ListMembersRequest(
    string? Search,
    SakinTipi? SakinTipi,
    BakiyeDurumu? BakiyeDurumu,
    bool? ActiveOnly,
    string? SortBy,
    string? SortDirection,
    int? Page,
    int? PageSize);
