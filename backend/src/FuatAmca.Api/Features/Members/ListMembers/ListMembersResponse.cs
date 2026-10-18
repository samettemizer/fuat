using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;

namespace FuatAmca.Api.Features.Members.ListMembers;

/// <summary>Uye datagrid satiri. Bakiye alanlari master bakiye tablosundan gelir.</summary>
public sealed record MemberRowResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    string? EvSahibiFullName,
    bool IsActive,
    decimal TahakkukToplam,
    decimal OdemeToplam,
    decimal Bakiye,
    BakiyeDurumu BakiyeDurumu,
    DateOnly? EnYakinMuhlet,
    bool MuhletGectiMi);

/// <summary>Sayfali uye listesi ve filtre kapsamindaki toplamlar.</summary>
public sealed record ListMembersResponse(
    PagedResult<MemberRowResponse> Page,
    decimal ToplamBorc,
    decimal ToplamAlacak,
    int BorcluSayisi,
    int AlacakliSayisi);
