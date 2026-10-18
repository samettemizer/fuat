namespace FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;

/// <summary>Guncellenen duyurunun son hali.</summary>
public sealed record DuyuruGuncelleResponse(
    Guid Id,
    Guid ApartmanId,
    string Title,
    string Content,
    DateOnly PublishedOn,
    DateOnly? ExpiresOn,
    bool IsActive);
