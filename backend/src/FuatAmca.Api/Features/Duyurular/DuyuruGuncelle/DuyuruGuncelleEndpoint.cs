using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Duyurular.DuyuruGuncelle;

internal static class DuyuruGuncelleEndpoint
{
    internal static RouteGroupBuilder MapDuyuruGuncelle(this RouteGroupBuilder group)
    {
        group.MapPut("/{duyuruId:guid}", async (
                Guid duyuruId,
                DuyuruGuncelleRequest request,
                DuyuruGuncelleHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, duyuruId, request, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .Validate<DuyuruGuncelleRequest>()
            .WithName("DuyuruGuncelle")
            .WithSummary("Duyuru metnini ve yayin penceresini gunceller")
            .Produces<DuyuruGuncelleResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
