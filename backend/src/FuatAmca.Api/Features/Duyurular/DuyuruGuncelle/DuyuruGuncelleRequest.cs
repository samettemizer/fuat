namespace FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;

/// <summary>Mevcut duyuru guncelleme istegi.</summary>
public sealed record DuyuruGuncelleRequest(
    Guid ApartmanId,
    string Title,
    string Content,
    DateOnly PublishedOn,
    DateOnly? ExpiresOn,
    bool IsActive);
