using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;

/// <summary>Guncellenen apartman ozeti.</summary>
public sealed record ApartmanGuncelleResponse(
    Guid Id,
    string Name,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    bool IsActive,
    bool YakitTutariSifirlandi);
