using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>
/// Uye listesi disa aktarim parametreleri (query string). Filtreler uye listesi ile aynidir,
/// sayfalama uygulanmaz. Format: "xlsx" (varsayilan) veya "pdf".
/// </summary>
public sealed record ExportMembersRequest(
    string? Format,
    string? Search,
    SakinTipi? SakinTipi,
    BakiyeDurumu? BakiyeDurumu,
    bool? ActiveOnly,
    string? SortBy,
    string? SortDirection);
