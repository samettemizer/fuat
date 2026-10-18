using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.PanelUsers.DeactivateUser;

/// <summary>
/// Panel kullanicisini pasife alir (kayit silinmez) ve acik refresh tokenlarini iptal eder.
/// Kendi hesabini ve sistemdeki son aktif yoneticiyi pasife almak engellidir.
/// </summary>
public sealed class DeactivateUserHandler(
    UserManager<User> userManager,
    FuatAmcaDbContext dbContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<DeactivateUserResponse>> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(k => k.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<DeactivateUserResponse>(
                Error.NotFound("user.not_found", "Kullanıcı kaydı bulunamadı."));
        }

        if (userId == currentUser.GetRequiredUserId())
        {
            return Result.Failure<DeactivateUserResponse>(
                Error.BusinessRule("user.self_deactivation", "Kendi hesabınızı pasife alamazsınız."));
        }

        var roles = await userManager.GetRolesAsync(user);
        if (user.IsActive && roles.Contains(FuatAmcaRoles.Admin, StringComparer.Ordinal))
        {
            var admins = await userManager.GetUsersInRoleAsync(FuatAmcaRoles.Admin);
            var hasOtherActiveAdmin = admins.Any(
                admin => admin.IsActive && admin.Id != userId);

            if (!hasOtherActiveAdmin)
            {
                return Result.Failure<DeactivateUserResponse>(
                    Error.BusinessRule("user.last_admin", "Sistemde en az bir aktif yönetici kalmalıdır."));
            }
        }

        var now = timeProvider.GetUtcNow();

        user.IsActive = false;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure<DeactivateUserResponse>(
                Error.Validation(
                    "user.update_failed",
                    string.Join(" ", updateResult.Errors.Select(error => error.Description))));
        }

        var revokedTokenCount = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken);

        return Result.Success(new DeactivateUserResponse(userId, user.IsActive, revokedTokenCount));
    }
}
