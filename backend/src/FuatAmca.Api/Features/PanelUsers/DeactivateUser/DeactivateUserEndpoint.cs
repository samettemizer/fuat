using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.DeactivateUser;

internal static class DeactivateUserEndpoint
{
    internal static RouteGroupBuilder MapDeactivateUser(this RouteGroupBuilder group)
    {
        group.MapDelete("/{userId:guid}", async (
                Guid userId,
                DeactivateUserHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(userId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .WithName("DeactivateUser")
            .WithSummary("Panel kullanicisini pasife alir")
            .WithDescription("Kayit silinmez; kullanici pasife alinir ve acik oturumlari kapatilir.")
            .Produces<DeactivateUserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }
}
