using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Authentication.Login;

/// <summary>
/// Eposta + parola ile giris yapar, RS256 erisim tokeni ve refresh token uretir.
/// Sistemde tek apartman varsa Yonetici rolundeki kullanici o apartmana otomatik yetkilendirilir.
/// </summary>
public sealed class LoginHandler(
    UserManager<User> userManager,
    FuatAmcaDbContext dbContext,
    TokenGenerator tokenGenerator,
    TimeProvider timeProvider,
    ILogger<LoginHandler> logger)
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("auth.invalid_credentials", "E-posta veya parola hatalı.");

    public async Task<Result<LoginResponse>> HandleAsync(
        LoginRequest request,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(
                Error.Forbidden("auth.inactive_user", "Hesabınız pasif durumda. Yönetici ile görüşünüz."));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<LoginResponse>(
                Error.Forbidden("auth.locked_account", "Çok sayıda hatalı deneme nedeniyle hesap geçici olarak kilitli."));
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            logger.LogWarning("Invalid login attempt. User: {UserId}", user.Id);
            return Result.Failure<LoginResponse>(InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        if (roles.Contains(FuatAmcaRoles.Admin, StringComparer.Ordinal))
        {
            await GrantSingleApartmanAccessAsync(user.Id, cancellationToken);
        }

        // Otomatik yetki kaydi (varsa) refresh token ile ayni transaction'da yazilir.
        var tokens = tokenGenerator.Generate(user, [.. roles], clientIp);
        await dbContext.SaveChangesAsync(cancellationToken);

        var apartmanlar = await dbContext.ApartmanUsers
            .AsNoTracking()
            .Where(ak => ak.UserId == user.Id && ak.Apartman!.IsActive)
            .OrderBy(ak => ak.Apartman!.Name)
            .Select(ak => new ApartmanOptionResponse(ak.ApartmanId, ak.Apartman!.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(new LoginResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt,
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            [.. roles],
            apartmanlar));
    }

    /// <summary>
    /// Sistemdeki tek apartmana bagli olmayan yoneticinin yetki kaydini ekler; kaydi SaveChanges'e birakir.
    /// Apartman panel disindan (SQL, veri aktarimi) eklendiginde yoneticinin ilk giriste yetkisiz kalip
    /// her ekranda 403 almasini onler. Birden fazla apartman varsa kimseye kendiliginden yetki verilmez.
    /// </summary>
    private async Task GrantSingleApartmanAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var apartmanIds = await dbContext.Apartmanlar
            .OrderBy(apartman => apartman.Id)
            .Select(apartman => apartman.Id)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (apartmanIds is not [var apartmanId])
        {
            return;
        }

        var hasAccess = await dbContext.ApartmanUsers
            .AnyAsync(ak => ak.ApartmanId == apartmanId && ak.UserId == userId, cancellationToken);

        if (hasAccess)
        {
            return;
        }

        dbContext.ApartmanUsers.Add(new ApartmanUser
        {
            ApartmanId = apartmanId,
            UserId = userId,
            AddedAt = timeProvider.GetUtcNow()
        });

        logger.LogInformation(
            "Granting admin access to the only apartman. User: {UserId}, Apartman: {ApartmanId}",
            userId,
            apartmanId);
    }
}
