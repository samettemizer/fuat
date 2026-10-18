using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketListesi;

internal static class HareketListesiEndpoint
{
    internal static RouteGroupBuilder MapHareketListesi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                Guid apartmanId,
                [AsParameters] HareketListesiRequest request,
                HareketListesiHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("HareketListesi")
            .WithSummary("Apartmanin hareket defterini filtreli ve sayfali listeler")
            .WithDescription("Islem tarihi azalan, esitlikte kayit tarihi azalan sirada doner; toplamlar filtrenin tamamina aittir.")
            .Produces<HareketListesiResponse>();

        return group;
    }
}
