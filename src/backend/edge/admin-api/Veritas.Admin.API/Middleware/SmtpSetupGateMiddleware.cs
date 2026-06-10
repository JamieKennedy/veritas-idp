using Microsoft.AspNetCore.Mvc;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Errors;

namespace Veritas.Admin.API.Middleware;

/// <summary>
/// Blocks authenticated protected admin workflows until SMTP setup has succeeded.
/// </summary>
public sealed class SmtpSetupGateMiddleware(RequestDelegate next)
{
    private static readonly PathString[] AllowedPrefixes =
    [
        "/api/v1/setup",
        "/api/v1/bootstrap",
        "/api/v1/messaging",
        "/api/v1/admin-auth/login",
        "/api/v1/admin-auth/logout",
        "/health",
        "/alive",
        "/openapi",
        "/scalar"
    ];

    /// <summary>
    /// Applies the SMTP setup gate to authenticated admin requests.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="smtpSetupStatus">The Messaging setup status reader.</param>
    /// <returns>A task that completes when the request is processed.</returns>
    public async Task InvokeAsync(HttpContext context, ISmtpSetupStatus smtpSetupStatus)
    {
        if (context.User.Identity?.IsAuthenticated != true ||
            IsAllowed(context.Request.Path) ||
            await smtpSetupStatus.IsSmtpConfiguredAsync(context.RequestAborted))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status428PreconditionRequired,
            Title = "Setup required.",
            Detail = "SMTP setup must be completed before this admin endpoint can be used.",
            Type = "https://httpstatuses.com/428"
        };
        problemDetails.Extensions["errorCode"] = SmtpNotConfiguredError.ErrorCode;
        problemDetails.Extensions["errorCategory"] = "Conflict";
        await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
    }

    private static bool IsAllowed(PathString path)
    {
        return AllowedPrefixes.Any(prefix => path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
