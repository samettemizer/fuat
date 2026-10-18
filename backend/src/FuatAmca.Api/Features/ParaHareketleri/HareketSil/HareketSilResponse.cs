using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketSil;

/// <summary>Silinen hareket ve uyenin silme sonrasi guncel bakiyesi.</summary>
public sealed record HareketSilResponse(
    Guid HareketId,
    Guid MemberId,
    decimal YeniBakiye,
    BakiyeDurumu BakiyeDurumu);
