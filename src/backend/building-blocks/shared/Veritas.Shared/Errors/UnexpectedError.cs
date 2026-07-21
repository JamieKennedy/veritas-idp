namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for unexpected operational failures.
/// </summary>
public abstract class UnexpectedError : VeritasError
{
    /// <summary>
    /// Initializes a new unexpected error.
    /// </summary>
    /// <param name="code">The unique stable error code.</param>
    /// <param name="message">The safe operational failure message.</param>
    protected UnexpectedError(string code, string message) : base(ErrorCategories.Unexpected, code, message)
    {
    }
}
