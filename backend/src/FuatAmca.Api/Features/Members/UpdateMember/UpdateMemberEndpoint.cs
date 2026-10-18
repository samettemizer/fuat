using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.UpdateMember;

internal static class UpdateMemberEndpoint
{
    internal static RouteGroupBuilder MapUpdateMember(this RouteGroupBuilder group)
    {
        group.MapPut("/{memberId:guid}", async (
                Guid memberId,
                UpdateMemberRequest request,
                UpdateMemberHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, memberId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .Validate<UpdateMemberRequest>()
            .WithName("UpdateMember")
            .WithSummary("Uye kaydini guncelle")
            .Produces<UpdateMemberResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
