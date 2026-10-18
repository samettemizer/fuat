using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.DeactivateMember;

internal static class DeactivateMemberEndpoint
{
    internal static RouteGroupBuilder MapDeactivateMember(this RouteGroupBuilder group)
    {
        group.MapDelete("/{memberId:guid}", async (
                Guid apartmanId,
                Guid memberId,
                DeactivateMemberHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, memberId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .WithName("DeactivateMember")
            .WithSummary("Uyeyi pasife alir")
            .WithDescription("Soft delete: kayit silinmez, IsActive alani false yapilir.")
            .Produces<DeactivateMemberResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
