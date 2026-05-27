namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for validation failures.
/// </summary>
public abstract class ValidationError : VeritasError
{
    /// <summary>
    /// Initializes a new validation error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe validation message.</param>
    protected ValidationError(string code, string message) : base(ErrorCategories.Validation, code, message)
    {
    }
}
