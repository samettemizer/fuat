using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketListesi;

/// <summary>Hareket defteri datagrid satiri.</summary>
public sealed record HareketRowResponse(
    Guid Id,
    Guid MemberId,
    string FullName,
    string DaireNo,
    GiderTipi GiderTipi,
    HareketYonu Yon,
    decimal Tutar,
    DateOnly IslemTarihi,
    int? DonemYil,
    int? DonemAy,
    DateOnly? Muhlet,
    string? Description,
    DateTimeOffset RecordedAt);

/// <summary>
/// Sayfali hareket listesi. Toplamlar sayfaya degil, filtrenin tamamina aittir.
/// </summary>
public sealed record HareketListesiResponse(
    PagedResult<HareketRowResponse> Page,
    decimal ToplamTahakkuk,
    decimal ToplamOdeme);
