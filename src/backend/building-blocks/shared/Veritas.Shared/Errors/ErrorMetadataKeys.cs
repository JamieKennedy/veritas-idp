namespace Veritas.Shared.Errors;

/// <summary>
/// Defines metadata keys attached to FluentResults errors across backend modules.
/// </summary>
public static class ErrorMetadataKeys
{
    /// <summary>
    /// Broad category used for HTTP status mapping.
    /// </summary>
    public const string Category = "ErrorCode";

    /// <summary>
    /// Unique stable error code used for tracing and client diagnostics.
    /// </summary>
    public const string Code = "Code";

    /// <summary>
    /// Optional explicit HTTP status code override.
    /// </summary>
    public const string HttpStatusCode = "HttpStatusCode";
}
