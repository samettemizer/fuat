using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanGuncelle;

internal static class ApartmanGuncelleEndpoint
{
    internal static RouteGroupBuilder MapApartmanGuncelle(this RouteGroupBuilder group)
    {
        group.MapPut("/{apartmanId:guid}", async (
                Guid apartmanId,
                ApartmanGuncelleRequest request,
                ApartmanGuncelleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.ApartmanManagement)
            .Validate<ApartmanGuncelleRequest>()
            .WithName("ApartmanGuncelle")
            .WithSummary("Apartman genel ayarlarini guncelle")
            .Produces<ApartmanGuncelleResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
