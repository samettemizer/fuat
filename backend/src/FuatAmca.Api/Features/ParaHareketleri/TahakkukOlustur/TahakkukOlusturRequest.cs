using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

/// <summary>
/// Aylik toplu tahakkuk uretim istegi.
/// GiderTipleri bos birakilirsa apartmanin aktif tum gider kalemleri islenir.
/// </summary>
public sealed record TahakkukOlusturRequest(
    Guid ApartmanId,
    int Yil,
    int Ay,
    IReadOnlyList<GiderTipi>? GiderTipleri,
    DateOnly? Muhlet,
    string? Description);
