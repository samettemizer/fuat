using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Authentication.RefreshToken;

/// <summary>
/// Refresh token rotasyonu: kullanilan token iptal edilir, yerine yenisi uretilir.
/// Daha once kullanilmis bir token tekrar gelirse kullanicinin tum tokenlari iptal edilir.
/// </summary>
public sealed class RefreshTokenHandler(
    FuatAmcaDbContext dbContext,
    TokenGenerator tokenGenerator,
    UserManager<User> userManager,
    TimeProvider timeProvider,
    ILogger<RefreshTokenHandler> logger)
{
    private static readonly Error InvalidRefreshToken =
        Error.Unauthorized("auth.invalid_refresh_token", "Yenileme tokeni geçersiz veya süresi dolmuş.");

    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenRequest request,
        string? clientIp,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var record = await tokenGenerator.FindRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (record is null)
        {
            return Result.Failure<RefreshTokenResponse>(InvalidRefreshToken);
        }

        if (record.UsedAt is not null)
        {
            // Token tekrar kullanilmis: sizinti varsayilir, tum aktif tokenlar iptal edilir.
            logger.LogWarning(
                "Refresh token reuse detected. User: {UserId}", record.UserId);

            await dbContext.RefreshTokens
                .Where(t => t.UserId == record.UserId && t.RevokedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(t => t.RevokedAt, now), cancellationToken);
            return Result.Failure<RefreshTokenResponse>(InvalidRefreshToken);
        }

        if (!record.IsActive(now))
        {
            return Result.Failure<RefreshTokenResponse>(InvalidRefreshToken);
        }

        var user = record.User;
        if (user is null || !user.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Forbidden("auth.inactive_user", "Hesabınız pasif durumda."));
        }

        record.UsedAt = now;
        record.RevokedAt = now;

        var roles = await userManager.GetRolesAsync(user);
        var tokens = tokenGenerator.Generate(user, [.. roles], clientIp, record.Id);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt));
    }
}
