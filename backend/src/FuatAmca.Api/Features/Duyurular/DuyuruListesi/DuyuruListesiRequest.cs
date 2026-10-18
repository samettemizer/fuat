namespace FuatAmca.Api.Features.Duyurular.DuyuruListesi;

/// <summary>
/// Duyuru listesi sorgu parametreleri.
/// SadeceAktif true ise yalnizca su an yayinda olan duyurular doner.
/// </summary>
public sealed record DuyuruListesiRequest(
    bool? ActiveOnly,
    int? Page,
    int? PageSize);
