using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketListesi;

/// <summary>
/// Hareket defteri filtreleri. Tum alanlar opsiyoneldir, query string ile gelir.
/// </summary>
public sealed record HareketListesiRequest(
    Guid? MemberId,
    GiderTipi? GiderTipi,
    HareketYonu? Yon,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int? DonemYil,
    int? DonemAy,
    int? Page,
    int? PageSize);
