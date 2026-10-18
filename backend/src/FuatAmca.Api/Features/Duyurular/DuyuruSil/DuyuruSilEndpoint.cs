using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Duyurular.DuyuruSil;

internal static class DuyuruSilEndpoint
{
    internal static RouteGroupBuilder MapDuyuruSil(this RouteGroupBuilder group)
    {
        group.MapDelete("/{duyuruId:guid}", async (
                Guid apartmanId,
                Guid duyuruId,
                DuyuruSilHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, duyuruId, cancellationToken);
                return result.ToHttpResult();
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .WithName("DuyuruSil")
            .WithSummary("Duyuru kaydini siler")
            .Produces<DuyuruSilResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
