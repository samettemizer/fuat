namespace FuatAmca.Api.Features.Duyurular.DuyuruOlustur;

/// <summary>Olusturulan duyuru ozeti.</summary>
public sealed record DuyuruOlusturResponse(
    Guid Id,
    Guid ApartmanId,
    string Title,
    string Content,
    DateOnly PublishedOn,
    DateOnly? ExpiresOn,
    bool IsActive,
    DateTimeOffset CreatedAt);
