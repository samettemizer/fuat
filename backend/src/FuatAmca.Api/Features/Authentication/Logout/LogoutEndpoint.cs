using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Authentication.Logout;

internal static class LogoutEndpoint
{
    internal static RouteGroupBuilder MapLogout(this RouteGroupBuilder group)
    {
        group.MapPost("/logout", async (
                LogoutRequest request,
                LogoutHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization()
            .Validate<LogoutRequest>()
            .WithName("Logout")
            .WithSummary("Oturumu kapatir, refresh tokenlari iptal eder")
            .Produces<LogoutResponse>();

        return group;
    }
}
