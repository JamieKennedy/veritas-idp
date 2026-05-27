namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for authorization failures.
/// </summary>
public abstract class ForbiddenError : VeritasError
{
    /// <summary>
    /// Initializes a new forbidden error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe authorization failure message.</param>
    protected ForbiddenError(string code, string message) : base(ErrorCategories.Forbidden, code, message)
    {
    }
}
