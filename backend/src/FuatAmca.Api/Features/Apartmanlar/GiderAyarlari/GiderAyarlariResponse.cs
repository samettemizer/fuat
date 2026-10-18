using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

/// <summary>Tek gider kaleminin ayarlari.</summary>
public sealed record GiderKalemiResponse(
    GiderTipi GiderTipi,
    decimal VarsayilanTutar,
    bool KiracidanAlinirMi,
    bool IsActive,
    DateTimeOffset UpdatedAt);

/// <summary>Apartmanin tum gider kalemi ayarlari.</summary>
public sealed record GiderAyarlariResponse(
    Guid ApartmanId,
    IsinmaTipi IsinmaTipi,
    IReadOnlyList<GiderKalemiResponse> Kalemler);
