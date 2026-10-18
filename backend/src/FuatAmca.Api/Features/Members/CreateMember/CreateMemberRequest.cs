using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.CreateMember;

/// <summary>Yeni uye kaydi olusturma istegi.</summary>
public sealed record CreateMemberRequest(
    Guid ApartmanId,
    string FirstName,
    string LastName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    string? EvSahibiFullName,
    string? EvSahibiPhoneNumber,
    string? Email,
    string? Notes);
