namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for authentication failures.
/// </summary>
public abstract class UnauthorizedError : VeritasError
{
    /// <summary>
    /// Initializes a new unauthorized error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe authentication failure message.</param>
    protected UnauthorizedError(string code, string message) : base(ErrorCategories.Unauthorized, code, message)
    {
    }
}
