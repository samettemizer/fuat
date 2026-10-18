using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.PanelUsers.CreateUser;

/// <summary>
/// Yeni panel kullanicisini Identity uzerinden olusturur, rollerini atar ve
/// yetkili oldugu apartmanlari baglar. Parola hash'i UserManager tarafindan uretilir.
/// </summary>
public sealed class CreateUserHandler(
    UserManager<User> userManager,
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<CreateUserResponse>> HandleAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return Result.Failure<CreateUserResponse>(
                Error.Conflict("user.email_already_registered", "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var."));
        }

        var roles = request.Roles
            .Select(role => role.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (Array.Exists(roles, role => !FuatAmcaRoles.IsValid(role)))
        {
            return Result.Failure<CreateUserResponse>(
                Error.Validation("user.invalid_role", "Tanımlı olmayan bir rol seçildi."));
        }

        var apartmanIds = request.ApartmanIds.Distinct().ToArray();

        // Kullanici sadece kendi yetkili oldugu apartmanlar icin yetki dagitabilir.
        var accessibleApartmanIds = await accessService.GetAccessibleApartmanIds().ToListAsync(cancellationToken);
        if (Array.Exists(apartmanIds, id => !accessibleApartmanIds.Contains(id)))
        {
            return Result.Failure<CreateUserResponse>(
                Error.Forbidden("apartman.access_denied", "Yetkiniz olmayan bir apartman için kullanıcı tanımlayamazsınız."));
        }

        var now = timeProvider.GetUtcNow();
        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = request.FullName.Trim(),
            IsActive = true,
            CreatedAt = now,
            CreatedByUserId = currentUser.GetRequiredUserId()
        };

        var creationResult = await userManager.CreateAsync(user, request.Password);
        if (!creationResult.Succeeded)
        {
            return Result.Failure<CreateUserResponse>(
                Error.Validation("user.creation_failed", JoinErrorDescriptions(creationResult)));
        }

        var roleResult = await userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
        {
            // Rol atamasi basarisizsa rolsuz / yetkisiz bir hesap birakmamak icin kaydi geri aliriz.
            await userManager.DeleteAsync(user);
            return Result.Failure<CreateUserResponse>(
                Error.Validation("user.creation_failed", JoinErrorDescriptions(roleResult)));
        }

        foreach (var apartmanId in apartmanIds)
        {
            dbContext.ApartmanUsers.Add(new ApartmanUser
            {
                ApartmanId = apartmanId,
                UserId = user.Id,
                AddedAt = now
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateUserResponse(
            user.Id,
            user.FullName,
            user.Email ?? email,
            roles,
            apartmanIds,
            user.CreatedAt));
    }

    private static string JoinErrorDescriptions(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
