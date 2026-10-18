using FuatAmca.Api.Common.Constants;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;

namespace FuatAmca.Api.Features.Members.ExportMembers;

internal static class ExportMembersEndpoint
{
    internal static RouteGroupBuilder MapExportMembers(this RouteGroupBuilder group)
    {
        group.MapGet("/export", async (
                Guid apartmanId,
                [AsParameters] ExportMembersRequest request,
                ExportMembersHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(apartmanId, request, cancellationToken);
                return result.ToHttpResult(value => Microsoft.AspNetCore.Http.Results.File(value.Content, value.ContentType, value.FileName));
            })
            .RequireAuthorization(FuatAmcaPolicies.Read)
            .WithName("ExportMembers")
            .WithSummary("Uye listesini xlsx veya pdf olarak indirir")
            .WithDescription("format=xlsx (varsayilan) veya format=pdf. Filtreler uye listesi ile aynidir, sayfalama uygulanmaz.")
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}
