using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.PanelUsers.ResetPassword;

/// <summary>
/// Yoneticinin bir panel kullanicisinin parolasini sifirlamasi. Parola hash'i
/// Identity tarafindan uretilir, kullanicinin acik refresh tokenlari iptal edilir.
/// </summary>
public sealed class ResetPasswordHandler(
    UserManager<User> userManager,
    FuatAmcaDbContext dbContext,
    TimeProvider timeProvider)
{
    public async Task<Result<ResetPasswordResponse>> HandleAsync(
        Guid userId,
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(k => k.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<ResetPasswordResponse>(
                Error.NotFound("user.not_found", "Kullanıcı kaydı bulunamadı."));
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure<ResetPasswordResponse>(
                Error.Validation(
                    "user.password_reset_failed",
                    string.Join(" ", result.Errors.Select(error => error.Description))));
        }

        var now = timeProvider.GetUtcNow();

        // Parola degistiginde acik oturumlar da kapanmali.
        var revokedTokenCount = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken);

        return Result.Success(new ResetPasswordResponse(userId, revokedTokenCount));
    }
}
