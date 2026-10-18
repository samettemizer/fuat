using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.UpdateUser;

internal static class UpdateUserEndpoint
{
    internal static RouteGroupBuilder MapUpdateUser(this RouteGroupBuilder group)
    {
        group.MapPut("/{userId:guid}", async (
                Guid userId,
                UpdateUserRequest request,
                UpdateUserHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(userId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .Validate<UpdateUserRequest>()
            .WithName("UpdateUser")
            .WithSummary("Panel kullanicisini gunceller")
            .WithDescription("Roller ve apartman yetkileri istege gore senkronize edilir.")
            .Produces<UpdateUserResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }
}
