using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

internal static class GiderAyarlariEndpoint
{
    internal static RouteGroupBuilder MapGiderAyarlariGetir(this RouteGroupBuilder group)
    {
        group.MapGet("/{apartmanId:guid}/gider-ayarlari", async (
                Guid apartmanId,
                GiderAyarlariGetirHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("GiderAyarlariGetir")
            .WithSummary("Apartmanin aylik gider kalemi ayarlarini dondurur")
            .Produces<GiderAyarlariResponse>();

        return group;
    }

    internal static RouteGroupBuilder MapGiderAyarlariGuncelle(this RouteGroupBuilder group)
    {
        group.MapPut("/{apartmanId:guid}/gider-ayarlari", async (
                Guid apartmanId,
                GiderAyarlariGuncelleRequest request,
                GiderAyarlariGuncelleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.ApartmanManagement)
            .Validate<GiderAyarlariGuncelleRequest>()
            .WithName("GiderAyarlariGuncelle")
            .WithSummary("Aylik gider kalemi ayarlarini guncelle")
            .Produces<GiderAyarlariResponse>();

        return group;
    }
}
