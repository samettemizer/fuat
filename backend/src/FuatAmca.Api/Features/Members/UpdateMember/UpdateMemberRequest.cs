using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.UpdateMember;

/// <summary>Mevcut uye kaydini guncelleme istegi.</summary>
public sealed record UpdateMemberRequest(
    Guid ApartmanId,
    string FirstName,
    string LastName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    string? EvSahibiFullName,
    string? EvSahibiPhoneNumber,
    string? Email,
    string? Notes,
    bool IsActive);
