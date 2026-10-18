using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

/// <summary>Guncellenecek tek gider kalemi.</summary>
public sealed record GiderKalemiGuncelleRequest(
    GiderTipi GiderTipi,
    decimal VarsayilanTutar,
    bool KiracidanAlinirMi,
    bool IsActive);

/// <summary>Gider ayarlari guncelleme istegi.</summary>
public sealed record GiderAyarlariGuncelleRequest(IReadOnlyList<GiderKalemiGuncelleRequest> Kalemler);
