using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.Shared.Security;
using Veritas.UserService.Application.DataTransferObjects;
using Veritas.UserService.Application.Interfaces;
using Veritas.UserService.Application.Persistence;
using Veritas.UserService.Domain.Entities;
using Veritas.UserService.Domain.Errors.AdminUsers;

namespace Veritas.UserService.Application.Services;

public sealed class AdminUserService : BaseService<AdminUserService>, IAdminUserService
{
    private static readonly TimeSpan LoginChallengeLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SessionIdleLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan SessionAbsoluteLifetime = TimeSpan.FromHours(8);
    private const int PasswordSaltBytes = 16;
    private const int PasswordHashBytes = 32;
    private const int PasswordHashIterations = 210_000;
    private const int MinimumPasswordLength = 12;
    private const int RecoveryCodeCount = 10;

    private readonly IUserDbContext _dbContext;
    private readonly ISecretProtector _secretProtector;
    private readonly TimeProvider _timeProvider;
    private readonly TotpService _totpService = new();

    public AdminUserService(
        ILogger<AdminUserService> logger,
        IUserDbContext dbContext,
        ISecretProtector secretProtector,
        TimeProvider timeProvider) : base(logger)
    {
        _dbContext = dbContext;
        _secretProtector = secretProtector;
        _timeProvider = timeProvider;
    }

    public AdminUserService(
        ILogger<AdminUserService> logger,
        IUserDbContext dbContext) : this(logger, dbContext, new UnavailableSecretProtector(), TimeProvider.System)
    {
    }

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

            var existingAdminCount = await _dbContext.AdminUsers.CountAsync(cancellationToken);

            if (existingAdminCount > 0)
            {
                return new InitialAdminAlreadyExistsError();
            }

            var utcNow = UtcNow();
            _dbContext.AdminUsers.Add(new AdminUser
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = HashPassword(password),
                Name = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
                InitialAdminSlot = 1,
                SecurityStamp = Guid.NewGuid(),
                CreatedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
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
            var adminUser = await FindPasswordValidatedAdminAsync(email, password, cancellationToken);

            return adminUser is null
                ? new InvalidAdminCredentialsError()
                : ToAuthenticationDto(adminUser);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to validate administrator credentials.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<AdminLoginChallengeDto>> StartAdminLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adminUser = await FindPasswordValidatedAdminAsync(email, password, cancellationToken);
            if (adminUser is null)
            {
                return new InvalidAdminCredentialsError();
            }

            var utcNow = UtcNow();
            EnsureSecurityStamp(adminUser);
            var challengeToken = GenerateSecretToken();
            var challenge = new AdminLoginChallenge
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUser.Id,
                ChallengeTokenHash = HashSecret(challengeToken),
                CreatedAtUtc = utcNow,
                ExpiresAtUtc = utcNow.Add(LoginChallengeLifetime)
            };
            string? totpSecret = null;
            string? provisioningUri = null;

            if (string.IsNullOrWhiteSpace(adminUser.MfaSecretProtected) || adminUser.MfaEnabledAtUtc is null)
            {
                challenge.Purpose = EAdminLoginChallengePurpose.MfaEnrollment;
                totpSecret = _totpService.GenerateSecret();
                challenge.PendingMfaSecretProtected = _secretProtector.Protect(totpSecret);
                provisioningUri = _totpService.BuildProvisioningUri(adminUser.Email, totpSecret);
            }
            else
            {
                challenge.Purpose = EAdminLoginChallengePurpose.MfaVerification;
            }

            _dbContext.AdminLoginChallenges.Add(challenge);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdminLoginChallengeDto(
                challenge.Id,
                challengeToken,
                challenge.Purpose,
                challenge.ExpiresAtUtc,
                totpSecret,
                provisioningUri);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to start administrator login.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<AdminMfaEnrollmentResultDto>> CompleteAdminMfaEnrollmentAsync(
        Guid challengeId,
        string challengeToken,
        string totpCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var challenge = await LoadValidChallengeAsync(
                challengeId,
                challengeToken,
                EAdminLoginChallengePurpose.MfaEnrollment,
                cancellationToken);
            if (challenge is null || string.IsNullOrWhiteSpace(challenge.PendingMfaSecretProtected))
            {
                return new InvalidAdminCredentialsError();
            }

            var adminUser = await _dbContext.AdminUsers.SingleOrDefaultAsync(
                user => user.Id == challenge.AdminUserId,
                cancellationToken);
            if (adminUser is null)
            {
                return new InvalidAdminCredentialsError();
            }

            var totpSecret = _secretProtector.Unprotect(challenge.PendingMfaSecretProtected);
            if (!_totpService.VerifyCode(totpSecret, totpCode, _timeProvider.GetUtcNow()))
            {
                return new InvalidAdminCredentialsError();
            }

            var utcNow = UtcNow();
            var recoveryCodes = GenerateRecoveryCodes();
            adminUser.MfaSecretProtected = challenge.PendingMfaSecretProtected;
            adminUser.MfaEnabledAtUtc = utcNow;
            adminUser.MfaUpdatedAtUtc = utcNow;
            adminUser.SecurityStamp = Guid.NewGuid();
            challenge.ConsumedAtUtc = utcNow;

            _dbContext.AdminRecoveryCodes.AddRange(recoveryCodes.Select(code => new AdminRecoveryCode
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUser.Id,
                CodeHash = HashSecret(code),
                CreatedAtUtc = utcNow
            }));
            var session = CreateSession(adminUser, utcNow);
            _dbContext.AdminSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdminMfaEnrollmentResultDto(
                ToAuthenticationDto(adminUser),
                session.Id,
                session.SecurityStamp,
                session.IdleExpiresAtUtc,
                session.AbsoluteExpiresAtUtc,
                recoveryCodes);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to complete administrator MFA enrollment.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<AdminSessionAuthenticationDto>> CompleteAdminMfaVerificationAsync(
        Guid challengeId,
        string challengeToken,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var challenge = await LoadValidChallengeAsync(
                challengeId,
                challengeToken,
                EAdminLoginChallengePurpose.MfaVerification,
                cancellationToken);
            if (challenge is null)
            {
                return new InvalidAdminCredentialsError();
            }

            var adminUser = await _dbContext.AdminUsers.SingleOrDefaultAsync(
                user => user.Id == challenge.AdminUserId,
                cancellationToken);
            if (adminUser?.MfaSecretProtected is null || adminUser.MfaEnabledAtUtc is null)
            {
                return new InvalidAdminCredentialsError();
            }

            var utcNow = UtcNow();
            var totpSecret = _secretProtector.Unprotect(adminUser.MfaSecretProtected);
            var isValidCode = _totpService.VerifyCode(totpSecret, code, _timeProvider.GetUtcNow()) ||
                              await TryConsumeRecoveryCodeAsync(adminUser.Id, code, utcNow, cancellationToken);
            if (!isValidCode)
            {
                return new InvalidAdminCredentialsError();
            }

            challenge.ConsumedAtUtc = utcNow;
            var session = CreateSession(adminUser, utcNow);
            _dbContext.AdminSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdminSessionAuthenticationDto(
                ToAuthenticationDto(adminUser),
                session.Id,
                session.SecurityStamp,
                session.IdleExpiresAtUtc,
                session.AbsoluteExpiresAtUtc);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to complete administrator MFA verification.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<AdminSessionAuthenticationDto>> ValidateAdminSessionAsync(
        Guid adminUserId,
        Guid sessionId,
        Guid securityStamp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var utcNow = UtcNow();
            var session = await _dbContext.AdminSessions.SingleOrDefaultAsync(
                candidate => candidate.Id == sessionId && candidate.AdminUserId == adminUserId,
                cancellationToken);
            var adminUser = await _dbContext.AdminUsers.SingleOrDefaultAsync(
                user => user.Id == adminUserId,
                cancellationToken);

            if (session is null ||
                adminUser is null ||
                session.RevokedAtUtc is not null ||
                session.SecurityStamp != securityStamp ||
                adminUser.SecurityStamp != securityStamp ||
                adminUser.MfaEnabledAtUtc is null ||
                string.IsNullOrWhiteSpace(adminUser.MfaSecretProtected) ||
                session.IdleExpiresAtUtc <= utcNow ||
                session.AbsoluteExpiresAtUtc <= utcNow)
            {
                return new InvalidAdminCredentialsError();
            }

            session.LastSeenAtUtc = utcNow;
            session.IdleExpiresAtUtc = MinUtc(utcNow.Add(SessionIdleLifetime), session.AbsoluteExpiresAtUtc);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new AdminSessionAuthenticationDto(
                ToAuthenticationDto(adminUser),
                session.Id,
                session.SecurityStamp,
                session.IdleExpiresAtUtc,
                session.AbsoluteExpiresAtUtc);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to validate administrator session.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result> RevokeAdminSessionAsync(
        Guid adminUserId,
        Guid sessionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _dbContext.AdminSessions.SingleOrDefaultAsync(
                candidate => candidate.Id == sessionId && candidate.AdminUserId == adminUserId,
                cancellationToken);
            if (session is null)
            {
                return Result.Ok();
            }

            if (session.RevokedAtUtc is null)
            {
                session.RevokedAtUtc = UtcNow();
                session.RevocationReason = string.IsNullOrWhiteSpace(reason) ? "unspecified" : reason.Trim();
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to revoke administrator session.");
            return new AdminCredentialValidationFailedError();
        }
    }

    /// <inheritdoc />
    public async Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _dbContext.AdminUsers
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
    /// Finds a tracked administrator after validating normalized email and password.
    /// </summary>
    /// <param name="email">The email address supplied by the caller.</param>
    /// <param name="password">The raw password supplied by the caller.</param>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns>The tracked administrator when credentials are valid; otherwise <see langword="null" />.</returns>
    private async Task<AdminUser?> FindPasswordValidatedAdminAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (normalizedEmail is null || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var adminUser = await _dbContext.AdminUsers
            .SingleOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        return adminUser is null || !VerifyPassword(password, adminUser.PasswordHash)
            ? null
            : adminUser;
    }

    /// <summary>
    /// Loads a valid unconsumed login challenge for the expected purpose.
    /// </summary>
    /// <param name="challengeId">The challenge identifier to load.</param>
    /// <param name="challengeToken">The raw challenge token to verify.</param>
    /// <param name="purpose">The expected challenge purpose.</param>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns>The challenge when it is valid; otherwise <see langword="null" />.</returns>
    private async Task<AdminLoginChallenge?> LoadValidChallengeAsync(
        Guid challengeId,
        string challengeToken,
        EAdminLoginChallengePurpose purpose,
        CancellationToken cancellationToken)
    {
        var challenge = await _dbContext.AdminLoginChallenges.SingleOrDefaultAsync(
            candidate => candidate.Id == challengeId,
            cancellationToken);
        if (challenge is null ||
            challenge.Purpose != purpose ||
            challenge.ConsumedAtUtc is not null ||
            challenge.ExpiresAtUtc <= UtcNow() ||
            !VerifySecret(challengeToken, challenge.ChallengeTokenHash))
        {
            return null;
        }

        return challenge;
    }

    /// <summary>
    /// Attempts to consume a matching unused recovery code.
    /// </summary>
    /// <param name="adminUserId">The administrator that owns the recovery code.</param>
    /// <param name="code">The raw recovery code supplied by the caller.</param>
    /// <param name="utcNow">The UTC timestamp to record when the code is consumed.</param>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns><see langword="true" /> when a matching recovery code was consumed.</returns>
    private async Task<bool> TryConsumeRecoveryCodeAsync(
        Guid adminUserId,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.AdminRecoveryCodes
            .Where(candidate => candidate.AdminUserId == adminUserId && candidate.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            if (!VerifySecret(code, candidate.CodeHash))
            {
                continue;
            }

            candidate.ConsumedAtUtc = utcNow;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new server-side administrator session.
    /// </summary>
    /// <param name="adminUser">The administrator that owns the session.</param>
    /// <param name="utcNow">The UTC timestamp when the session is issued.</param>
    /// <returns>The new session entity.</returns>
    private static AdminSession CreateSession(AdminUser adminUser, DateTime utcNow)
    {
        EnsureSecurityStamp(adminUser);
        var absoluteExpiresAtUtc = utcNow.Add(SessionAbsoluteLifetime);

        return new AdminSession
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUser.Id,
            SecurityStamp = adminUser.SecurityStamp,
            IssuedAtUtc = utcNow,
            LastSeenAtUtc = utcNow,
            IdleExpiresAtUtc = MinUtc(utcNow.Add(SessionIdleLifetime), absoluteExpiresAtUtc),
            AbsoluteExpiresAtUtc = absoluteExpiresAtUtc
        };
    }

    /// <summary>
    /// Ensures an administrator has a non-empty security stamp.
    /// </summary>
    /// <param name="adminUser">The administrator to update when needed.</param>
    private static void EnsureSecurityStamp(AdminUser adminUser)
    {
        if (adminUser.SecurityStamp == Guid.Empty)
        {
            adminUser.SecurityStamp = Guid.NewGuid();
        }
    }

    /// <summary>
    /// Creates a safe administrator authentication DTO.
    /// </summary>
    /// <param name="adminUser">The administrator entity to map.</param>
    /// <returns>The safe administrator identity.</returns>
    private static AdminUserAuthenticationDto ToAuthenticationDto(AdminUser adminUser)
    {
        return new AdminUserAuthenticationDto(adminUser.Id, adminUser.Email, adminUser.Name);
    }

    /// <summary>
    /// Gets the current UTC timestamp without offset information.
    /// </summary>
    /// <returns>The current UTC timestamp.</returns>
    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    /// <summary>
    /// Generates one-time recovery codes for administrator MFA recovery.
    /// </summary>
    /// <returns>The raw recovery codes to show once to the administrator.</returns>
    private static IReadOnlyList<string> GenerateRecoveryCodes()
    {
        return Enumerable.Range(0, RecoveryCodeCount)
            .Select(_ => $"veritas-{Base32Encoding.Encode(RandomNumberGenerator.GetBytes(10)).ToLowerInvariant()}")
            .ToList();
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

    /// <summary>
    /// Generates a high-entropy token suitable for login challenges.
    /// </summary>
    /// <returns>The generated token.</returns>
    private static string GenerateSecretToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>
    /// Hashes a one-time secret with SHA-256 and a per-secret salt.
    /// </summary>
    /// <param name="secret">The raw secret value.</param>
    /// <returns>A versioned salted SHA-256 hash.</returns>
    private static string HashSecret(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = SHA256.HashData(salt.Concat(System.Text.Encoding.UTF8.GetBytes(secret)).ToArray());

        return $"v1.sha256.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a raw one-time secret against a stored salted hash.
    /// </summary>
    /// <param name="secret">The raw secret supplied by the caller.</param>
    /// <param name="storedHash">The stored versioned salted hash.</param>
    /// <returns><see langword="true" /> when the secret matches the stored hash.</returns>
    private static bool VerifySecret(string secret, string storedHash)
    {
        var parts = storedHash.Split('.', 4);
        if (parts.Length != 4 || parts[0] != "v1" || parts[1] != "sha256")
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = SHA256.HashData(salt.Concat(System.Text.Encoding.UTF8.GetBytes(secret)).ToArray());

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the earlier of two UTC timestamps.
    /// </summary>
    /// <param name="left">The first timestamp.</param>
    /// <param name="right">The second timestamp.</param>
    /// <returns>The earlier timestamp.</returns>
    private static DateTime MinUtc(DateTime left, DateTime right)
    {
        return left <= right ? left : right;
    }

    private sealed class UnavailableSecretProtector : ISecretProtector
    {
        /// <inheritdoc />
        public string Protect(string plaintext)
        {
            throw new InvalidOperationException("Secret protection is required for administrator MFA operations.");
        }

        /// <inheritdoc />
        public string Unprotect(string protectedText)
        {
            throw new InvalidOperationException("Secret protection is required for administrator MFA operations.");
        }
    }
}
