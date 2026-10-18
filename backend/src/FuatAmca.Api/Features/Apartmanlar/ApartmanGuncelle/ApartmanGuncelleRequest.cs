using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;

/// <summary>Apartman genel ayarlarini guncelleme istegi.</summary>
public sealed record ApartmanGuncelleRequest(
    string Name,
    string? Address,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    bool IsActive);
