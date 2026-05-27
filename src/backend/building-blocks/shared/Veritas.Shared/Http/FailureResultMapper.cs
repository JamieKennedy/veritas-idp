using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veritas.Shared.Errors;

namespace Veritas.Shared.Http;

/// <summary>
/// Maps application failures to RFC 7807 ProblemDetails responses.
/// </summary>
public static class FailureResultMapper
{
    private const string ErrorCodeExtensionKey = "errorCode";
    private const string ErrorCategoryExtensionKey = "errorCategory";
    private const string GenericFailureDetail = "The request failed unexpectedly.";

    /// <summary>
    /// Maps a failed result to an HTTP ProblemDetails response.
    /// </summary>
    /// <param name="result">The failed result to map.</param>
    /// <returns>An action result containing ProblemDetails.</returns>
    public static IActionResult ToProblemDetails(ResultBase result)
    {
        var firstError = result.Errors.FirstOrDefault();
        var statusCode = ResolveStatusCode(firstError);
        var detail = statusCode >= StatusCodes.Status500InternalServerError
            ? GenericFailureDetail
            : firstError?.Message ?? "The request is invalid.";
        var category = ResolveCategory(firstError);
        var code = ResolveCode(firstError);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = ResolveTitle(statusCode),
            Detail = detail,
            Type = ResolveType(statusCode)
        };

        if (code is not null)
        {
            problemDetails.Extensions[ErrorCodeExtensionKey] = code;
        }

        if (category is not null)
        {
            problemDetails.Extensions[ErrorCategoryExtensionKey] = category;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates a validation ProblemDetails response for transport-level validation failures.
    /// </summary>
    /// <param name="detail">The validation detail safe to return to the caller.</param>
    /// <returns>An action result containing validation ProblemDetails.</returns>
    public static IActionResult ValidationProblem(string detail)
    {
        return ToProblemDetails(Result.Fail(new TransportValidationError(detail)));
    }

    /// <summary>
    /// Creates an unauthorized ProblemDetails response for authentication failures.
    /// </summary>
    /// <param name="detail">The authentication failure detail safe to return to the caller.</param>
    /// <returns>An action result containing authentication ProblemDetails.</returns>
    public static IActionResult UnauthorizedProblem(string detail)
    {
        return ToProblemDetails(Result.Fail(new TransportUnauthorizedError(detail)));
    }

    /// <summary>
    /// Resolves the HTTP status code for a result error.
    /// </summary>
    /// <param name="error">The first result error.</param>
    /// <returns>The HTTP status code to return.</returns>
    private static int ResolveStatusCode(IError? error)
    {
        if (error?.Metadata.TryGetValue(ErrorMetadataKeys.HttpStatusCode, out var explicitStatusCode) == true &&
            int.TryParse(explicitStatusCode?.ToString(), out var parsedStatusCode))
        {
            return parsedStatusCode;
        }

        return ResolveCategory(error) switch
        {
            ErrorCategories.Validation => StatusCodes.Status400BadRequest,
            ErrorCategories.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategories.Forbidden => StatusCodes.Status403Forbidden,
            ErrorCategories.NotFound => StatusCodes.Status404NotFound,
            ErrorCategories.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Resolves the stable unique error code from a result error.
    /// </summary>
    /// <param name="error">The first result error.</param>
    /// <returns>The unique error code, or <see langword="null" /> when unavailable.</returns>
    private static string? ResolveCode(IError? error)
    {
        if (error?.Metadata.TryGetValue(ErrorMetadataKeys.Code, out var code) != true)
        {
            return null;
        }

        return code?.ToString();
    }

    /// <summary>
    /// Resolves the broad error category metadata value from a result error.
    /// </summary>
    /// <param name="error">The first result error.</param>
    /// <returns>The error category, or <see langword="null" /> when unavailable.</returns>
    private static string? ResolveCategory(IError? error)
    {
        if (error?.Metadata.TryGetValue(ErrorMetadataKeys.Category, out var category) != true)
        {
            return null;
        }

        return category?.ToString();
    }

    /// <summary>
    /// Resolves the ProblemDetails title for a status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A short ProblemDetails title.</returns>
    private static string ResolveTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Validation failed.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status403Forbidden => "Forbidden.",
            StatusCodes.Status404NotFound => "Not found.",
            StatusCodes.Status409Conflict => "Conflict.",
            StatusCodes.Status410Gone => "Gone.",
            _ => "Unexpected failure."
        };
    }

    /// <summary>
    /// Resolves the ProblemDetails type URI for a status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A ProblemDetails type URI.</returns>
    private static string ResolveType(int statusCode)
    {
        return $"https://httpstatuses.com/{statusCode}";
    }

    private sealed class TransportValidationError(string detail)
        : ValidationError("TRANSPORT_VALIDATION_FAILED", detail);

    private sealed class TransportUnauthorizedError(string detail)
        : UnauthorizedError("TRANSPORT_UNAUTHORIZED", detail);
}
