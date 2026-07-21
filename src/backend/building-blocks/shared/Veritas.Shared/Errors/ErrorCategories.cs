namespace Veritas.Shared.Errors;

/// <summary>
/// Defines broad error categories used by API hosts to map domain failures to transport responses.
/// </summary>
public static class ErrorCategories
{
    /// <summary>
    /// The request failed validation.
    /// </summary>
    public const string Validation = nameof(Validation);

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    public const string NotFound = nameof(NotFound);

    /// <summary>
    /// The request conflicts with current state.
    /// </summary>
    public const string Conflict = nameof(Conflict);

    /// <summary>
    /// Authentication is required or credentials are invalid.
    /// </summary>
    public const string Unauthorized = nameof(Unauthorized);

    /// <summary>
    /// The authenticated caller is not allowed to perform the operation.
    /// </summary>
    public const string Forbidden = nameof(Forbidden);

    /// <summary>
    /// An unexpected operational failure occurred.
    /// </summary>
    public const string Unexpected = nameof(Unexpected);
}
