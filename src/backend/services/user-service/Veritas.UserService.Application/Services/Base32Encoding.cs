namespace Veritas.UserService.Application.Services;

/// <summary>
/// Encodes and decodes RFC 4648 Base32 values used for TOTP shared secrets.
/// </summary>
internal static class Base32Encoding
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>
    /// Encodes binary data as an unpadded Base32 string.
    /// </summary>
    /// <param name="data">The binary data to encode.</param>
    /// <returns>The unpadded Base32 representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data" /> is <see langword="null" />.</exception>
    public static string Encode(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var output = new char[(int)Math.Ceiling(data.Length * 8 / 5d)];
        var buffer = 0;
        var bitsLeft = 0;
        var outputIndex = 0;

        foreach (var value in data)
        {
            buffer = (buffer << 8) | value;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                output[outputIndex++] = Alphabet[(buffer >> (bitsLeft - 5)) & 0x1F];
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            output[outputIndex] = Alphabet[(buffer << (5 - bitsLeft)) & 0x1F];
        }

        return new string(output);
    }

    /// <summary>
    /// Decodes an unpadded or padded Base32 string.
    /// </summary>
    /// <param name="value">The Base32 value to decode.</param>
    /// <returns>The decoded binary payload.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="value" /> contains non-Base32 characters.</exception>
    public static byte[] Decode(string value)
    {
        var bits = 0;
        var bitBuffer = 0;
        var bytes = new List<byte>();

        foreach (var character in value.TrimEnd('=').ToUpperInvariant())
        {
            var index = Alphabet.IndexOf(character, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new FormatException("Invalid Base32 character.");
            }

            bitBuffer = (bitBuffer << 5) | index;
            bits += 5;

            if (bits < 8)
            {
                continue;
            }

            bytes.Add((byte)((bitBuffer >> (bits - 8)) & 0xFF));
            bits -= 8;
        }

        return [.. bytes];
    }
}
