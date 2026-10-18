namespace FuatAmca.Api.Features.Duyurular.DuyuruListesi;

/// <summary>Duyuru listesi satiri.</summary>
public sealed record DuyuruRowResponse(
    Guid Id,
    Guid ApartmanId,
    string Title,
    string Content,
    DateOnly PublishedOn,
    DateOnly? ExpiresOn,
    bool IsActive,
    DateTimeOffset CreatedAt);
