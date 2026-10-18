using FuatAmca.Api.Features.Authentication.GetCurrentUser;
using FuatAmca.Api.Features.Authentication.Logout;
using FuatAmca.Api.Features.Authentication.Login;
using FuatAmca.Api.Features.Authentication.RefreshToken;

namespace FuatAmca.Api.Features.Authentication;

/// <summary>
/// Authentication endpointleri: /auth
/// </summary>
public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapLogin();
        group.MapRefreshToken();
        group.MapLogout();
        group.MapGetCurrentUser();

        return endpoints;
    }
}
