namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for state conflict failures.
/// </summary>
public abstract class ConflictError : VeritasError
{
    /// <summary>
    /// Initializes a new conflict error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe conflict message.</param>
    protected ConflictError(string code, string message) : base(ErrorCategories.Conflict, code, message)
    {
    }
}
