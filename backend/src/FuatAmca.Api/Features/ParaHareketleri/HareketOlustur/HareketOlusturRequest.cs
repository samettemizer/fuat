using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;

/// <summary>
/// Tek bir para hareketi (tahakkuk veya odeme) kayit istegi.
/// Donem alanlari tahakkuk yonunde zorunlu, odemede opsiyoneldir.
/// </summary>
public sealed record HareketOlusturRequest(
    Guid ApartmanId,
    Guid MemberId,
    GiderTipi GiderTipi,
    HareketYonu Yon,
    decimal Tutar,
    DateOnly? IslemTarihi,
    int? DonemYil,
    int? DonemAy,
    DateOnly? Muhlet,
    string? Description);
