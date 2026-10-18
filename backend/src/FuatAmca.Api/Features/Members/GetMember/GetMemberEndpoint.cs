using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.GetMember;

internal static class GetMemberEndpoint
{
    internal static RouteGroupBuilder MapGetMember(this RouteGroupBuilder group)
    {
        group.MapGet("/{memberId:guid}", async (
                Guid apartmanId,
                Guid memberId,
                GetMemberHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, memberId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("GetMember")
            .WithSummary("Uye detayini, bakiye kirilimini ve son hareketleri dondurur")
            .Produces<GetMemberResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
