namespace FuatAmca.Api.Common.Validation;

/// <summary>
/// Endpoint tanimlarinda kullanilan kisayol.
/// </summary>
public static class ValidationEndpointExtensions
{
    /// <summary>Istek govdesini kayitli IValidator ile dogrular.</summary>
    public static RouteHandlerBuilder Validate<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class =>
        builder
            .AddEndpointFilter<ValidationEndpointFilter<TRequest>>()
            .ProducesValidationProblem();
}
