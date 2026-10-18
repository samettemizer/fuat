using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Authentication.RefreshToken;

internal static class RefreshTokenEndpoint
{
    internal static RouteGroupBuilder MapRefreshToken(this RouteGroupBuilder group)
    {
        group.MapPost("/refresh-token", async (
                RefreshTokenRequest request,
                RefreshTokenHandler handler,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    request,
                    context.Connection.RemoteIpAddress?.ToString(),
                    cancellationToken);

                return result.ToHttpResult();
            })
            .AllowAnonymous()
            .Validate<RefreshTokenRequest>()
            .WithName("RefreshToken")
            .WithSummary("Refresh token ile yeni erisim tokeni uretir")
            .Produces<RefreshTokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }
}
