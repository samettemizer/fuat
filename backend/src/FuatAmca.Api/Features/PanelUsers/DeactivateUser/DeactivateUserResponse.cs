namespace FuatAmca.Api.Features.PanelUsers.DeactivateUser;

/// <summary>Pasife alinan kullanicinin son durumu.</summary>
public sealed record DeactivateUserResponse(
    Guid UserId,
    bool IsActive,
    int RevokedTokenCount);
