using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.ListUsers;

internal static class ListUsersEndpoint
{
    internal static RouteGroupBuilder MapListUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                [AsParameters] ListUsersRequest request,
                ListUsersHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .WithName("ListUsers")
            .WithSummary("Panel kullanicilarini listeler")
            .WithDescription("Sadece istegi yapanin yetkili oldugu apartmanlarda yetkisi olan kullanicilar doner.")
            .Produces<PagedResult<UserRowResponse>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
