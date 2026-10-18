using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.ParaHareketleri.TahakkukOlustur;

internal static class TahakkukOlusturEndpoint
{
    internal static RouteGroupBuilder MapTahakkukOlustur(this RouteGroupBuilder group)
    {
        group.MapPost("/tahakkuk", async (
                TahakkukOlusturRequest request,
                TahakkukOlusturHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.ParaHareketiManagement)
            .Validate<TahakkukOlusturRequest>()
            .WithName("TahakkukOlustur")
            .WithSummary("Secilen donem icin aylik tahakkuklari uretir")
            .WithDescription("Idempotenttir: donemde kaydi bulunan uye ve gider kalemi kombinasyonlari atlanir.")
            .Produces<TahakkukOlusturResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }
}
