namespace FuatAmca.Api.Features.Duyurular.DuyuruOlustur;

/// <summary>Yeni duyuru olusturma istegi. Yayin tarihi bos ise bugun kabul edilir.</summary>
public sealed record DuyuruOlusturRequest(
    Guid ApartmanId,
    string Title,
    string Content,
    DateOnly? PublishedOn,
    DateOnly? ExpiresOn);
