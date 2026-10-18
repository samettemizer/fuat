using FuatAmca.Api.Common.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FuatAmca.Api.Common.Results;

/// <summary>
/// <see cref="Result"/> nesnelerini RFC 9457 ProblemDetails uyumlu HTTP yanitlarina cevirir.
/// </summary>
public static class ResultHttpMapping
{
    public static IResult ToHttpResult(this Result result, Func<IResult>? successFactory = null) =>
        result.IsSuccess
            ? successFactory?.Invoke() ?? Microsoft.AspNetCore.Http.Results.NoContent()
            : CreateProblem(result.Error!);

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult>? successFactory = null) =>
        result.IsSuccess
            ? successFactory?.Invoke(result.Value!) ?? Microsoft.AspNetCore.Http.Results.Ok(result.Value)
            : CreateProblem(result.Error!);

    /// <summary>Hata turune gore HTTP durum kodu.</summary>
    public static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        _ => StatusCodes.Status400BadRequest
    };

    private static ProblemHttpResult CreateProblem(Error error) => TypedResults.Problem(
        detail: error.Message,
        statusCode: GetStatusCode(error.Type),
        title: GetTitle(error.Type),
        type: $"https://fuat.local/errors/{error.Code}",
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Geçersiz istek",
        ErrorType.Unauthorized => "Kimlik doğrulaması gerekli",
        ErrorType.Forbidden => "Bu kayda erişim yetkiniz yok",
        ErrorType.NotFound => "Kayıt bulunamadı",
        ErrorType.Conflict => "Kayıt çakışması",
        ErrorType.BusinessRule => "İş kuralı ihlali",
        _ => "İstek işlenemedi"
    };
}
