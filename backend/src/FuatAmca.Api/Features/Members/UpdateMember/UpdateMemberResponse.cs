using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.UpdateMember;

/// <summary>Guncellenen uye ozeti.</summary>
public sealed record UpdateMemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    string? EvSahibiFullName,
    string? EvSahibiPhoneNumber,
    string? Email,
    bool IsActive,
    DateTimeOffset? UpdatedAt);
