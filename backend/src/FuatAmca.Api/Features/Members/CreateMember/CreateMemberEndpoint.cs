using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.CreateMember;

internal static class CreateMemberEndpoint
{
    internal static RouteGroupBuilder MapCreateMember(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
                CreateMemberRequest request,
                CreateMemberHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request.ApartmanId, request, cancellationToken);
                return result.ToHttpResult(value =>
                    Microsoft.AspNetCore.Http.Results.Created($"/members/{value.Id}", value));
            })
            .RequireAuthorization(FuatAmcaPolicies.MemberManagement)
            .Validate<CreateMemberRequest>()
            .WithName("CreateMember")
            .WithSummary("Apartmana yeni uye ekler")
            .WithDescription("Cep telefonu apartman icinde tekildir; uye ile birlikte sifir bakiyeli master bakiye kaydi acilir.")
            .Produces<CreateMemberResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }
}
