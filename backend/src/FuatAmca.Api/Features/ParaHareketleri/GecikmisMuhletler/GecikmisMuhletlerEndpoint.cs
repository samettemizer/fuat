using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.ParaHareketleri.GecikmisMuhletler;

internal static class GecikmisMuhletlerEndpoint
{
    internal static RouteGroupBuilder MapGecikmisMuhletler(this RouteGroupBuilder group)
    {
        group.MapGet("/gecikmis-muhletler", async (
                Guid apartmanId,
                GecikmisMuhletlerHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("GecikmisMuhletler")
            .WithSummary("Muhleti gecmis ve halen borclu uyeleri listeler")
            .WithDescription("Panel bu listeyi acilista toast uyarisi gostermek icin cagirir; muhlet tarihi artan sirada doner.")
            .Produces<IReadOnlyList<GecikmisMuhletResponse>>();

        return group;
    }
}
