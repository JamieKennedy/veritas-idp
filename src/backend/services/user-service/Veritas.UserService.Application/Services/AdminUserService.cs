using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using Veritas.UserService.Application.DataTransferObjects;
using Veritas.UserService.Application.Interfaces;
using Veritas.UserService.Application.Persistence;
using Veritas.UserService.Domain.Entities;
using Veritas.UserService.Domain.Errors.AdminUsers;

namespace Veritas.UserService.Application.Services;

public sealed class AdminUserService(
    ILogger<AdminUserService> logger,
    IUserDbContext dbContext) : BaseService<AdminUserService>(logger), IAdminUserService
{
    private const int PasswordSaltBytes = 16;
    private const int PasswordHashBytes = 32;
    private const int PasswordHashIterations = 210_000;
    private const int MinimumPasswordLength = 12;

    /// <inheritdoc />
    public async Task<Result> CreateInitialAdminUserAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = NormalizeEmail(email);

            if (normalizedEmail is null)
            {
                return new InvalidAdminEmailError();
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < MinimumPasswordLength)
            {
                return new AdminPasswordTooShortError(MinimumPasswordLength);
            }

            var existingAdminCount = await dbContext.AdminUsers.CountAsync(cancellationToken);

            if (existingAdminCount > 0)
            {
                return new InitialAdminAlreadyExistsError();
            }

            var utcNow = DateTime.UtcNow;
            dbContext.AdminUsers.Add(new AdminUser
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = HashPassword(password),
                Name = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
                InitialAdminSlot = 1,
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (DbUpdateException exception)
        {
            Logger.LogError(exception, "Failed to create initial administrator account due to a database update error.");
            return new InitialAdminCreationFailedError();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to create initial administrator account.");
            return new InitialAdminCreationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<AdminUserAuthenticationDto>> ValidateAdminCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = NormalizeEmail(email);

            if (normalizedEmail is null || string.IsNullOrWhiteSpace(password))
            {
                return new InvalidAdminCredentialsError();
            }

            var adminUser = await dbContext.AdminUsers
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

            if (adminUser is null || !VerifyPassword(password, adminUser.PasswordHash))
            {
                return new InvalidAdminCredentialsError();
            }

            return new AdminUserAuthenticationDto(adminUser.Id, adminUser.Email, adminUser.Name);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to validate administrator credentials.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await dbContext.AdminUsers
                .AsNoTracking()
                .CountAsync(cancellationToken);

            return Result.Ok(count);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to get admin user count.");
            return new AdminUserCountUnavailableError();
        }
    }

    /// <summary>
    /// Normalizes an administrator email address for uniqueness checks and persistence.
    /// </summary>
    /// <param name="email">The email address supplied by the caller.</param>
    /// <returns>The normalized email address, or <see langword="null" /> when the value is invalid.</returns>
    private static string? NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        try
        {
            var trimmedEmail = email.Trim();
            var address = new MailAddress(trimmedEmail);

            if (!string.Equals(address.Address, trimmedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return address.Address.Trim().ToLowerInvariant();
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>
    /// Hashes a raw administrator password with PBKDF2-SHA256 and a per-password salt.
    /// </summary>
    /// <param name="password">The raw password to hash.</param>
    /// <returns>A versioned password hash string containing algorithm parameters, salt, and derived hash.</returns>
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(PasswordSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            PasswordHashIterations,
            HashAlgorithmName.SHA256,
            PasswordHashBytes);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"v1.pbkdf2-sha256.{PasswordHashIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}");
    }

    /// <summary>
    /// Verifies a raw password against a stored versioned password hash.
    /// </summary>
    /// <param name="password">The raw password supplied by the caller.</param>
    /// <param name="storedHash">The stored versioned password hash.</param>
    /// <returns><see langword="true" /> when the password matches the stored hash.</returns>
    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.', 5);

        if (parts.Length != 5 ||
            parts[0] != "v1" ||
            parts[1] != "pbkdf2-sha256" ||
            !int.TryParse(parts[2], CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
