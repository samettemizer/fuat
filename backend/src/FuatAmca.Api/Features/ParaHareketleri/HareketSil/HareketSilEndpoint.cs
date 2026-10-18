using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketSil;

internal static class HareketSilEndpoint
{
    internal static RouteGroupBuilder MapHareketSil(this RouteGroupBuilder group)
    {
        group.MapDelete("/{hareketId:guid}", async (
                Guid apartmanId,
                Guid hareketId,
                HareketSilHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, hareketId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.ParaHareketiManagement)
            .WithName("HareketSil")
            .WithSummary("Para hareketini siler")
            .WithDescription("Silme sonrasi uyenin bakiyesi hareket defterinden bastan hesaplanir.")
            .Produces<HareketSilResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
