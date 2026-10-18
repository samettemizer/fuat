using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.PanelUsers.UpdateUser;

/// <summary>
/// Panel kullanicisinin bilgilerini, rollerini ve apartman yetkilerini senkronize eder.
/// Kendi hesabini pasife alma ve son aktif yoneticiyi dusurme girisimleri engellenir.
/// </summary>
public sealed class UpdateUserHandler(
    UserManager<User> userManager,
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<UpdateUserResponse>> HandleAsync(
        Guid userId,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(k => k.ApartmanGrants)
            .FirstOrDefaultAsync(k => k.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UpdateUserResponse>(
                Error.NotFound("user.not_found", "Kullanıcı kaydı bulunamadı."));
        }

        if (!request.IsActive && userId == currentUser.GetRequiredUserId())
        {
            return Result.Failure<UpdateUserResponse>(
                Error.BusinessRule("user.self_deactivation", "Kendi hesabınızı pasife alamazsınız."));
        }

        var requestedRoles = request.Roles
            .Select(role => role.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (Array.Exists(requestedRoles, role => !FuatAmcaRoles.IsValid(role)))
        {
            return Result.Failure<UpdateUserResponse>(
                Error.Validation("user.invalid_role", "Tanımlı olmayan bir rol seçildi."));
        }

        var requestedApartmanIds = request.ApartmanIds.Distinct().ToArray();
        var accessibleApartmanIds = await accessService.GetAccessibleApartmanIds().ToListAsync(cancellationToken);
        if (Array.Exists(requestedApartmanIds, id => !accessibleApartmanIds.Contains(id)))
        {
            return Result.Failure<UpdateUserResponse>(
                Error.Forbidden("apartman.access_denied", "Yetkiniz olmayan bir apartman için yetki veremezsiniz."));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var isAdminBeingRemoved = currentRoles.Contains(FuatAmcaRoles.Admin, StringComparer.Ordinal)
            && (!requestedRoles.Contains(FuatAmcaRoles.Admin, StringComparer.Ordinal) || !request.IsActive);

        if (isAdminBeingRemoved && !await HasOtherActiveAdminAsync(userId))
        {
            return Result.Failure<UpdateUserResponse>(
                Error.BusinessRule("user.last_admin", "Sistemde en az bir aktif yönetici kalmalıdır."));
        }

        var now = timeProvider.GetUtcNow();

        user.FullName = request.FullName.Trim();
        user.IsActive = request.IsActive;

        var currentApartmanIds = user.ApartmanGrants
            .Select(grant => grant.ApartmanId)
            .ToHashSet();

        // Istegi yapanin goremedigi apartman yetkileri oldugu gibi korunur.
        foreach (var grant in user.ApartmanGrants
                     .Where(grant => accessibleApartmanIds.Contains(grant.ApartmanId)
                         && !requestedApartmanIds.Contains(grant.ApartmanId))
                     .ToList())
        {
            dbContext.ApartmanUsers.Remove(grant);
        }

        foreach (var apartmanId in requestedApartmanIds.Where(id => !currentApartmanIds.Contains(id)))
        {
            dbContext.ApartmanUsers.Add(new ApartmanUser
            {
                ApartmanId = apartmanId,
                UserId = user.Id,
                AddedAt = now
            });
        }

        var rolesToRemove = currentRoles.Except(requestedRoles, StringComparer.Ordinal).ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removalResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removalResult.Succeeded)
            {
                return Result.Failure<UpdateUserResponse>(
                    Error.Validation("user.update_failed", JoinErrorDescriptions(removalResult)));
            }
        }

        var rolesToAdd = requestedRoles.Except(currentRoles, StringComparer.Ordinal).ToArray();
        if (rolesToAdd.Length > 0)
        {
            var additionResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!additionResult.Succeeded)
            {
                return Result.Failure<UpdateUserResponse>(
                    Error.Validation("user.update_failed", JoinErrorDescriptions(additionResult)));
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result.Failure<UpdateUserResponse>(
                Error.Validation("user.update_failed", JoinErrorDescriptions(updateResult)));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Pasife alinan kullanicinin acik oturumlari da kapatilir.
        var revokedTokenCount = request.IsActive
            ? 0
            : await dbContext.RefreshTokens
                .Where(token => token.UserId == userId && token.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken);

        var finalApartmanIds = await dbContext.ApartmanUsers
            .AsNoTracking()
            .Where(grant => grant.UserId == userId)
            .Select(grant => grant.ApartmanId)
            .ToListAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.IsActive,
            requestedRoles,
            finalApartmanIds,
            revokedTokenCount));
    }

    /// <summary>Verilen kullanici disinda aktif bir Yonetici kaldi mi.</summary>
    private async Task<bool> HasOtherActiveAdminAsync(Guid userId)
    {
        var admins = await userManager.GetUsersInRoleAsync(FuatAmcaRoles.Admin);
        return admins.Any(admin => admin.IsActive && admin.Id != userId);
    }

    private static string JoinErrorDescriptions(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
