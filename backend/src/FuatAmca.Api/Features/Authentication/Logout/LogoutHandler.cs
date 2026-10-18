using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Authentication.Logout;

/// <summary>
/// Refresh tokenlari iptal eder (oturum kapatma).
/// </summary>
public sealed class LogoutHandler(
    FuatAmcaDbContext dbContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<LogoutResponse>> HandleAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var now = timeProvider.GetUtcNow();

        var query = dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null);

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var hash = TokenGenerator.HashToken(request.RefreshToken);
            query = query.Where(t => t.TokenHash == hash);
        }

        var revokedTokenCount = await query.ExecuteUpdateAsync(
            update => update.SetProperty(t => t.RevokedAt, now),
            cancellationToken);

        return Result.Success(new LogoutResponse(revokedTokenCount));
    }
}
