namespace Veritas.Shared.Security;

/// <summary>
/// Protects reversible operational secrets before they are persisted.
/// </summary>
public interface ISecretProtector
{
    /// <summary>
    /// Protects plaintext so it can be stored outside process memory.
    /// </summary>
    /// <param name="plaintext">The raw secret value to protect.</param>
    /// <returns>The protected secret payload.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="plaintext" /> is empty.</exception>
    string Protect(string plaintext);

    /// <summary>
    /// Recovers plaintext from a previously protected payload.
    /// </summary>
    /// <param name="protectedText">The protected secret payload.</param>
    /// <returns>The recovered plaintext secret.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="protectedText" /> is empty.</exception>
    string Unprotect(string protectedText);
}
