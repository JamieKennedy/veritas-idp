using System.Diagnostics;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Veritas.Admin.API.Extensions;

/// <summary>
/// Converts unexpected request failures into safe, traceable HTTP responses.
/// </summary>
internal sealed class AdminApiExceptionHandler(
    ILogger<AdminApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Exception messages may include data from database or messaging providers.
        logger.LogError("Admin API request failed with {ExceptionType}. Trace ID: {TraceId}", exception.GetType().Name, traceId);

        var status = httpContext.Response.StatusCode;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = status >= StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : "The request could not be processed.",
            Extensions = { ["traceId"] = traceId }
        };

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(problem, options: null, "application/problem+json", cancellationToken);
        }

        return true;
    }
}
