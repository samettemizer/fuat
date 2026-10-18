using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace FuatAmca.Api.Common.DependencyInjection;

/// <summary>
/// OpenAPI dokumanina Bearer semasini ekler; boylece Scalar
/// arayuzunden endpointler token ile denenebilir.
/// </summary>
internal sealed class OpenApiSecurityTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "FuatAmca API",
            Version = "v1",
            Description = "Apartman / site yonetimi icin CMS ve REST API."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Panel kullanicisi JWT erisim tokeni."
        };

        return Task.CompletedTask;
    }
}
