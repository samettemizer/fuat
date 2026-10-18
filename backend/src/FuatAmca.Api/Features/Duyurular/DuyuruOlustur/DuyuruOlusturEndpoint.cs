using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Duyurular.DuyuruOlustur;

internal static class DuyuruOlusturEndpoint
{
    internal static RouteGroupBuilder MapDuyuruOlustur(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
                DuyuruOlusturRequest request,
                DuyuruOlusturHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, request, cancellationToken);
                return result.ToHttpResult(value =>
                    Microsoft.AspNetCore.Http.Results.Created($"/duyurular/{value.Id}", value));
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .Validate<DuyuruOlusturRequest>()
            .WithName("DuyuruOlustur")
            .WithSummary("Apartmana yeni duyuru ekler")
            .Produces<DuyuruOlusturResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return group;
    }
}
