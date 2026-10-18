using FuatAmca.Api.Features.ParaHareketleri.GecikmisMuhletler;
using FuatAmca.Api.Features.ParaHareketleri.HareketListesi;
using FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;
using FuatAmca.Api.Features.ParaHareketleri.HareketSil;
using FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

namespace FuatAmca.Api.Features.ParaHareketleri;

public static class ParaHareketiEndpoints
{
    public static IEndpointRouteBuilder MapParaHareketiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/para-hareketleri")
            .WithTags("Para Hareketleri")
            .RequireAuthorization();

        group.MapHareketOlustur();
        group.MapHareketListesi();
        group.MapHareketSil();
        group.MapTahakkukOlustur();
        group.MapGecikmisMuhletler();

        return endpoints;
    }
}
