namespace FuatAmca.Api.Features.PanelUsers.ResetPassword;

/// <summary>Parola sifirlama sonucu; acik oturumlar da kapatilir.</summary>
public sealed record ResetPasswordResponse(Guid UserId, int RevokedTokenCount);
