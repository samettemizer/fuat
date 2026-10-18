using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.PanelUsers.ResetPassword;

internal static class ResetPasswordEndpoint
{
    internal static RouteGroupBuilder MapResetPassword(this RouteGroupBuilder group)
    {
        group.MapPost("/{userId:guid}/reset-password", async (
                Guid userId,
                ResetPasswordRequest request,
                ResetPasswordHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(userId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.UserManagement)
            .Validate<ResetPasswordRequest>()
            .WithName("ResetPassword")
            .WithSummary("Panel kullanicisinin parolasini sifirlar")
            .WithDescription("Parola Identity uzerinden yenilenir ve kullanicinin acik refresh tokenlari iptal edilir.")
            .Produces<ResetPasswordResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
