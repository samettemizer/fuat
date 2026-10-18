using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketOlustur;

internal static class HareketOlusturEndpoint
{
    internal static RouteGroupBuilder MapHareketOlustur(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
                HareketOlusturRequest request,
                HareketOlusturHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, request, cancellationToken);
                return result.ToHttpResult(value =>
                    Microsoft.AspNetCore.Http.Results.Created($"/para-hareketleri/{value.HareketId}", value));
            })
            .RequireAuthorization(FuatAmcaPolicies.ParaHareketiManagement)
            .Validate<HareketOlusturRequest>()
            .WithName("HareketOlustur")
            .WithSummary("Uyeye tek tahakkuk veya odeme kaydeder")
            .WithDescription("Hareket kaydedilirken uyenin master bakiye tablolari ayni transaction icinde guncellenir.")
            .Produces<HareketOlusturResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
