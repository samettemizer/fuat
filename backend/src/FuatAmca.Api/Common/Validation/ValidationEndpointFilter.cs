using FluentValidation;

namespace FuatAmca.Api.Common.Validation;

/// <summary>
/// FluentValidation dogrulayicisini Minimal API endpoint filtresi olarak calistirir.
/// Deprecated FluentValidation.AspNetCore auto-validation paketi kullanilmaz.
/// </summary>
internal sealed class ValidationEndpointFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
    where TRequest : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (validationResult.IsValid)
        {
            return await next(context);
        }

        return TypedResults.ValidationProblem(
            errors: validationResult.ToDictionary(),
            title: "Doğrulama hatası",
            type: "https://fuat.local/errors/validation");
    }
}
