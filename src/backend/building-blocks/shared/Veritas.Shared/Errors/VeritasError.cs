using FluentResults;

namespace Veritas.Shared.Errors;

/// <summary>
/// Base type for Veritas domain/application errors with shared metadata conventions.
/// </summary>
public abstract class VeritasError : Error
{
    /// <summary>
    /// Initializes a new Veritas error.
    /// </summary>
    /// <param name="category">The broad category used for transport mapping.</param>
    /// <param name="code">The unique stable code used for tracing.</param>
    /// <param name="message">The safe error message.</param>
    protected VeritasError(string category, string code, string message) : base(message)
    {
        Metadata[ErrorMetadataKeys.Category] = category;
        Metadata[ErrorMetadataKeys.Code] = code;
    }
}
