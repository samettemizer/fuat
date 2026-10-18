using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.GetMember;

/// <summary>Uyenin gider kalemi bazinda bakiye kirilimi.</summary>
public sealed record MemberBakiyeKalemiResponse(
    GiderTipi GiderTipi,
    decimal TahakkukToplam,
    decimal OdemeToplam,
    decimal Bakiye);

/// <summary>Uye detayinda gosterilen son para hareketi satiri.</summary>
public sealed record MemberRecentHareketResponse(
    DateOnly IslemTarihi,
    GiderTipi GiderTipi,
    HareketYonu Yon,
    decimal Tutar,
    int? DonemYil,
    int? DonemAy,
    DateOnly? Muhlet,
    string? Description);

/// <summary>Uye detay yaniti: kimlik bilgileri, bakiye ozeti, kalemler ve son hareketler.</summary>
public sealed record GetMemberResponse(
    Guid Id,
    Guid ApartmanId,
    string FirstName,
    string LastName,
    string FullName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    string? EvSahibiFullName,
    string? EvSahibiPhoneNumber,
    string? Email,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    decimal TahakkukToplam,
    decimal OdemeToplam,
    decimal Bakiye,
    BakiyeDurumu BakiyeDurumu,
    DateOnly? EnYakinMuhlet,
    bool MuhletGectiMi,
    IReadOnlyList<MemberBakiyeKalemiResponse> Kalemler,
    IReadOnlyList<MemberRecentHareketResponse> RecentHareketler);
