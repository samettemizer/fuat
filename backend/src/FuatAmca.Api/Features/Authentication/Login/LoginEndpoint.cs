using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Authentication.Login;

internal static class LoginEndpoint
{
    internal static RouteGroupBuilder MapLogin(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (
                LoginRequest request,
                LoginHandler handler,
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
            .Validate<LoginRequest>()
            .WithName("Login")
            .WithSummary("Panele giris yapar")
            .WithDescription("E-posta ve parola ile kisa omurlu erisim tokeni ve refresh token uretir.")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return group;
    }
}
