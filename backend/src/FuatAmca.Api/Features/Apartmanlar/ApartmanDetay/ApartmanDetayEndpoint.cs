using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanDetay;

internal static class ApartmanDetayEndpoint
{
    internal static RouteGroupBuilder MapApartmanDetay(this RouteGroupBuilder group)
    {
        group.MapGet("/{apartmanId:guid}", async (
                Guid apartmanId,
                ApartmanDetayHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("ApartmanDetay")
            .WithSummary("Apartman detayini ve gider ayarlarini dondurur")
            .Produces<ApartmanDetayResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
