using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.CreateMember;

/// <summary>Olusturulan uye ozeti.</summary>
public sealed record CreateMemberResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string DaireNo,
    string PhoneNumber,
    SakinTipi SakinTipi,
    bool IsActive,
    DateTimeOffset CreatedAt);
