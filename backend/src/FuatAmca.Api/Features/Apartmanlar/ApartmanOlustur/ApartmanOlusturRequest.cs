using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;

/// <summary>Apartman olusturulurken girilen aylik gider tutarlari.</summary>
public sealed record GiderTutarlariRequest(
    decimal Aidat,
    decimal Yakit,
    decimal BinaTadilati,
    decimal SairGider);

/// <summary>Yeni apartman olusturma istegi.</summary>
public sealed record ApartmanOlusturRequest(
    string Name,
    string? Address,
    IsinmaTipi IsinmaTipi,
    int SonOdemeGunu,
    GiderTutarlariRequest? GiderTutarlari);
