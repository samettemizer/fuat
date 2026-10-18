using FuatAmca.Api.Features.Apartmanlar.ApartmanDetay;
using FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;
using FuatAmca.Api.Features.Apartmanlar.ApartmanListesi;
using FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;
using FuatAmca.Api.Features.Apartmanlar.GiderAyarlari;

namespace FuatAmca.Api.Features.Apartmanlar;

public static class ApartmanEndpoints
{
    public static IEndpointRouteBuilder MapApartmanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/apartmanlar")
            .WithTags("Apartmanlar")
            .RequireAuthorization();

        group.MapApartmanOlustur();
        group.MapApartmanListesi();
        group.MapApartmanDetay();
        group.MapApartmanGuncelle();
        group.MapGiderAyarlariGetir();
        group.MapGiderAyarlariGuncelle();

        return endpoints;
    }
}
