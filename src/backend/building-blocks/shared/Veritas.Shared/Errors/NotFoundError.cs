namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for missing resource failures.
/// </summary>
public abstract class NotFoundError : VeritasError
{
    /// <summary>
    /// Initializes a new not-found error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe not-found message.</param>
    protected NotFoundError(string code, string message) : base(ErrorCategories.NotFound, code, message)
    {
    }
}
