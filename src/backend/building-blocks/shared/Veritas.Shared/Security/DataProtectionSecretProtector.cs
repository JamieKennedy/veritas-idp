using Microsoft.AspNetCore.DataProtection;

namespace Veritas.Shared.Security;

/// <summary>
/// Protects secrets using ASP.NET Core Data Protection.
/// </summary>
public sealed class DataProtectionSecretProtector(IDataProtectionProvider dataProtectionProvider) : ISecretProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("Veritas.Shared.Secrets.v1");

    /// <inheritdoc />
    public string Protect(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            throw new ArgumentException("Secret plaintext is required.", nameof(plaintext));
        }

        return _protector.Protect(plaintext);
    }

    /// <inheritdoc />
    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrWhiteSpace(protectedText))
        {
            throw new ArgumentException("Protected secret text is required.", nameof(protectedText));
        }

        return _protector.Unprotect(protectedText);
    }
}
