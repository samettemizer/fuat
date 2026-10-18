using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;

/// <summary>Olusturulan apartman ozeti.</summary>
public sealed record ApartmanOlusturResponse(
    Guid Id,
    string Name,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    DateTimeOffset CreatedAt);
