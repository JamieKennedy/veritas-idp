namespace Veritas.PlatformService.Application.Interfaces;

/// <summary>
/// Validates deployment bootstrap secrets without exposing secret material to callers.
/// </summary>
public interface IBootstrapSecretValidator
{
    /// <summary>
    /// Gets a value indicating whether bootstrap secret material is configured.
    /// </summary>
    bool IsConfigured
    {
        get;
    }

    /// <summary>
    /// Determines whether a supplied bootstrap secret matches the configured secret.
    /// </summary>
    /// <param name="candidateSecret">The secret candidate supplied by the caller.</param>
    /// <returns><see langword="true" /> when the candidate matches the configured secret.</returns>
    bool IsValid(string candidateSecret);
}
