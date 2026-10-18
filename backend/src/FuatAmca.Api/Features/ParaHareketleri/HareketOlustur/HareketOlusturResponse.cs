using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;

/// <summary>
/// Kaydedilen hareketin ozeti ve uyenin hareket sonrasi guncel bakiyesi.
/// </summary>
public sealed record HareketOlusturResponse(
    Guid HareketId,
    Guid MemberId,
    string FullName,
    string DaireNo,
    GiderTipi GiderTipi,
    HareketYonu Yon,
    decimal Tutar,
    DateOnly IslemTarihi,
    DateOnly? Muhlet,
    decimal YeniBakiye,
    BakiyeDurumu BakiyeDurumu,
    bool MuhletGectiMi);
