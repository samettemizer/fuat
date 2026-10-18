using FuatAmca.Api.Features.PanelUsers.UpdateUser;
using FuatAmca.Api.Features.PanelUsers.ListUsers;
using FuatAmca.Api.Features.PanelUsers.CreateUser;
using FuatAmca.Api.Features.PanelUsers.DeactivateUser;
using FuatAmca.Api.Features.PanelUsers.ResetPassword;
using FuatAmca.Api.Features.PanelUsers.ListRoles;

namespace FuatAmca.Api.Features.PanelUsers;

public static class PanelUserEndpoints
{
    public static IEndpointRouteBuilder MapPanelUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/panel-users")
            .WithTags("Panel Users")
            .RequireAuthorization();

        group.MapCreateUser();
        group.MapListUsers();
        group.MapUpdateUser();
        group.MapResetPassword();
        group.MapDeactivateUser();

        // Rol listesi ayri bir kok yolda yayinlanir.
        var roleGroup = endpoints.MapGroup("/roles")
            .WithTags("Panel Users")
            .RequireAuthorization();

        roleGroup.MapListRoles();

        return endpoints;
    }
}
