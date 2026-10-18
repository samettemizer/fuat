using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanDetay;

/// <summary>Apartman gider ayari ozeti.</summary>
public sealed record ApartmanGiderAyariResponse(
    GiderTipi GiderTipi,
    decimal VarsayilanTutar,
    bool KiracidanAlinirMi,
    bool IsActive);

/// <summary>Apartman detay yaniti.</summary>
public sealed record ApartmanDetayResponse(
    Guid Id,
    string Name,
    string? Address,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    bool IsActive,
    DateTimeOffset CreatedAt,
    int MemberCount,
    decimal ToplamBorc,
    IReadOnlyList<ApartmanGiderAyariResponse> GiderAyarlari);
