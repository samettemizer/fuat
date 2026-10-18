using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FuatAmca.Api.Common.Exceptions;

/// <summary>
/// Beklenmeyen exception'lari RFC 9457 ProblemDetails olarak dondurur.
/// Beklenen hatalar Result pattern ile tasinir, buraya dusmez.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception. Path: {Path} {Method}",
            context.Request.Path,
            context.Request.Method);

        context.Response.StatusCode = exception switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = "https://fuat.local/errors/unexpected",
                Title = context.Response.StatusCode == StatusCodes.Status400BadRequest
                    ? "Geçersiz istek"
                    : "Sunucu hatası",
                Detail = context.Response.StatusCode == StatusCodes.Status400BadRequest
                    ? exception.Message
                    : "İstek işlenirken beklenmeyen bir hata oluştu.",
                Status = context.Response.StatusCode
            }
        });
    }
}
