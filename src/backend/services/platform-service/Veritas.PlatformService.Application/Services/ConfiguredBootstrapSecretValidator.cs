using System.Security.Cryptography;
using System.Text;

using Veritas.PlatformService.Application.Interfaces;

namespace Veritas.PlatformService.Application.Services;

/// <summary>
/// Validates bootstrap secret candidates against configured deployment secret material.
/// </summary>
public sealed class ConfiguredBootstrapSecretValidator : IBootstrapSecretValidator
{
    private readonly byte[] _configuredSecretHash;

    public ConfiguredBootstrapSecretValidator(string? configuredSecret)
    {
        _configuredSecretHash = string.IsNullOrWhiteSpace(configuredSecret)
            ? []
            : HashSecret(configuredSecret);
    }

    /// <inheritdoc />
    public bool IsConfigured => _configuredSecretHash.Length > 0;

    /// <inheritdoc />
    public bool IsValid(string candidateSecret)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(candidateSecret))
        {
            return false;
        }

        var candidateHash = HashSecret(candidateSecret);
        return CryptographicOperations.FixedTimeEquals(candidateHash, _configuredSecretHash);
    }

    /// <summary>
    /// Hashes bootstrap secret material before comparison.
    /// </summary>
    /// <param name="secret">The secret material to hash.</param>
    /// <returns>A fixed-length SHA-256 hash.</returns>
    private static byte[] HashSecret(string secret)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(secret.Trim()));
    }
}
