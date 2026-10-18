using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Authentication.GetCurrentUser;

internal static class GetCurrentUserEndpoint
{
    internal static RouteGroupBuilder MapGetCurrentUser(this RouteGroupBuilder group)
    {
        group.MapGet("/me", async (GetCurrentUserHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Oturum acmis kullanicinin profilini dondurur")
            .Produces<GetCurrentUserResponse>();

        return group;
    }
}
