using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;

internal static class ApartmanOlusturEndpoint
{
    internal static RouteGroupBuilder MapApartmanOlustur(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
                ApartmanOlusturRequest request,
                ApartmanOlusturHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.ToHttpResult(value => Microsoft.AspNetCore.Http.Results.Created($"/apartmanlar/{value.Id}", value));
            })
            .RequireAuthorization(FuatAmcaPolicies.ApartmanManagement)
            .Validate<ApartmanOlusturRequest>()
            .WithName("ApartmanOlustur")
            .WithSummary("Yeni apartman olusturur")
            .WithDescription("Apartmani olusturan kullanici otomatik olarak yetkili atanir, dort temel gider kalemi hazirlanir.")
            .Produces<ApartmanOlusturResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
