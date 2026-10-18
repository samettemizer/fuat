namespace FuatAmca.Api.Features.Members.DeactivateMember;

/// <summary>Pasife alinan uyenin son durumu.</summary>
public sealed record DeactivateMemberResponse(Guid Id, bool IsActive);
