using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.ListRoles;

internal static class ListRolesEndpoint
{
    internal static RouteGroupBuilder MapListRoles(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ListRolesHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .WithName("ListRoles")
            .WithSummary("Panel rollerini listeler")
            .Produces<IReadOnlyList<RoleResponse>>();

        return group;
    }
}
