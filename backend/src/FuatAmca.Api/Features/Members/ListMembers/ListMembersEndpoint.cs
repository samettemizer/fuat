using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.ListMembers;

internal static class ListMembersEndpoint
{
    internal static RouteGroupBuilder MapListMembers(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                Guid apartmanId,
                [AsParameters] ListMembersRequest request,
                ListMembersHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("ListMembers")
            .WithSummary("Apartmanin uyelerini filtreli ve sayfali listeler")
            .WithDescription("Filtreler: search, sakinTipi, bakiyeDurumu, activeOnly. Siralama: daireNo, fullName, bakiye, createdAt.")
            .Produces<ListMembersResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
