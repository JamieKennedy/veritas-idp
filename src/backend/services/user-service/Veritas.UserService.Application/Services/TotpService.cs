using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Veritas.UserService.Application.Services;

/// <summary>
/// Generates and verifies RFC 6238 TOTP codes for administrator MFA.
/// </summary>
internal static class TotpService
{
    private const int SecretByteLength = 20;
    private const int CodeDigits = 6;
    private const int TimeStepSeconds = 30;

    /// <summary>
    /// Generates a new Base32-encoded TOTP shared secret.
    /// </summary>
    /// <returns>The generated Base32 secret.</returns>
    public static string GenerateSecret()
    {
        return Base32Encoding.Encode(RandomNumberGenerator.GetBytes(SecretByteLength));
    }

    /// <summary>
    /// Builds an otpauth URI suitable for authenticator app enrollment.
    /// </summary>
    /// <param name="adminEmail">The normalized administrator email address.</param>
    /// <param name="secretBase32">The Base32 TOTP shared secret.</param>
    /// <returns>The otpauth provisioning URI.</returns>
    public static string BuildProvisioningUri(string adminEmail, string secretBase32)
    {
        var issuer = Uri.EscapeDataString("Veritas");
        var label = Uri.EscapeDataString($"Veritas:{adminEmail}");

        return $"otpauth://totp/{label}?secret={secretBase32}&issuer={issuer}&digits={CodeDigits}&period={TimeStepSeconds}";
    }

    /// <summary>
    /// Verifies a TOTP code against the current time with a one-step clock-skew window.
    /// </summary>
    /// <param name="secretBase32">The Base32 TOTP shared secret.</param>
    /// <param name="code">The user-supplied TOTP code.</param>
    /// <param name="utcNow">The current UTC timestamp.</param>
    /// <returns><see langword="true" /> when the code is valid for the allowed time window.</returns>
    public static bool VerifyCode(string secretBase32, string code, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != CodeDigits || code.Any(character => !char.IsDigit(character)))
        {
            return false;
        }

        var key = Base32Encoding.Decode(secretBase32);
        var currentCounter = utcNow.ToUnixTimeSeconds() / TimeStepSeconds;

        for (var offset = -1; offset <= 1; offset++)
        {
            var expectedCode = GenerateCode(key, currentCounter + offset);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(expectedCode),
                    Encoding.ASCII.GetBytes(code)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Generates a six-digit TOTP code for a binary secret and time counter.
    /// </summary>
    /// <param name="key">The binary TOTP shared secret.</param>
    /// <param name="counter">The time-step counter.</param>
    /// <returns>The six-digit TOTP code.</returns>
    private static string GenerateCode(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

#pragma warning disable CA5350 // RFC 6238 authenticator interoperability requires the standardized HMAC-SHA1 algorithm.
        using var hmac = new HMACSHA1(key);
#pragma warning restore CA5350
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);
        var code = binary % 1_000_000;

        return code.ToString("D6", CultureInfo.InvariantCulture);
    }
}
