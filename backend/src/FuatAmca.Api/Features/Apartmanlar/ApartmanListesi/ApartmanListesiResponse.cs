using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanListesi;

/// <summary>Panelde apartman secimi ve ozet gostergeleri icin satir.</summary>
public sealed record ApartmanRowResponse(
    Guid Id,
    string Name,
    string? Address,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    bool IsActive,
    int MemberCount,
    decimal ToplamBorc,
    decimal ToplamAlacak);
