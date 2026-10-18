using FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;
using FuatAmca.Api.Features.Duyurular.DuyuruListesi;
using FuatAmca.Api.Features.Duyurular.DuyuruOlustur;
using FuatAmca.Api.Features.Duyurular.DuyuruSil;

namespace FuatAmca.Api.Features.Duyurular;

public static class DuyuruEndpoints
{
    public static IEndpointRouteBuilder MapDuyuruEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/duyurular")
            .WithTags("Duyurular")
            .RequireAuthorization();

        group.MapDuyuruOlustur();
        group.MapDuyuruListesi();
        group.MapDuyuruGuncelle();
        group.MapDuyuruSil();

        return endpoints;
    }
}
