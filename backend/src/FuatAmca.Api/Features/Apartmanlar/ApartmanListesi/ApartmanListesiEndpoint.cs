using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanListesi;

internal static class ApartmanListesiEndpoint
{
    internal static RouteGroupBuilder MapApartmanListesi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ApartmanListesiHandler handler, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("ApartmanListesi")
            .WithSummary("Kullanicinin yetkili oldugu apartmanlari listeler")
            .Produces<IReadOnlyList<ApartmanRowResponse>>();

        return group;
    }
}
