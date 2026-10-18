using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Duyurular.DuyuruListesi;

internal static class DuyuruListesiEndpoint
{
    internal static RouteGroupBuilder MapDuyuruListesi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                Guid apartmanId,
                [AsParameters] DuyuruListesiRequest request,
                DuyuruListesiHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("DuyuruListesi")
            .WithSummary("Apartmanin duyurularini sayfali listeler")
            .Produces<PagedResult<DuyuruRowResponse>>()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
