using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.CreateUser;

internal static class CreateUserEndpoint
{
    internal static RouteGroupBuilder MapCreateUser(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
                CreateUserRequest request,
                CreateUserHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.ToHttpResult(value =>
                    Microsoft.AspNetCore.Http.Results.Created($"/panel-users/{value.UserId}", value));
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .Validate<CreateUserRequest>()
            .WithName("CreateUser")
            .WithSummary("Yeni panel kullanicisi olusturur")
            .WithDescription("Kullanicinin rolleri atanir ve sadece istegi yapanin yetkili oldugu apartmanlar baglanabilir.")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
