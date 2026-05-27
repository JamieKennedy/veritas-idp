using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Veritas.PlatformService.Application.DataTransferObjects.Setup;
using Veritas.PlatformService.Application.Dependencies;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Persistence;
using Veritas.PlatformService.Domain.Entities;
using Veritas.PlatformService.Domain.Errors.Base;
using Veritas.PlatformService.Domain.Errors.Bootstrap;
using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Application.Services;

public class BootstrapService : BaseService<BootstrapService>, IBootstrapService
{
    private const string BootstrapCompletedFlagKey = "BOOTSTRAP_COMPLETED";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(15);
    private readonly IPlatformDbContext _dbContext;
    private readonly IAdminUserDirectory _adminUserDirectory;
    private readonly IInitialAdminCreator _initialAdminCreator;
    private readonly IBootstrapSecretValidator _bootstrapSecretValidator;

    public BootstrapService(
        ILogger<BootstrapService> logger,
        IPlatformDbContext dbContext,
        IAdminUserDirectory adminUserDirectory,
        IInitialAdminCreator initialAdminCreator,
        IBootstrapSecretValidator bootstrapSecretValidator) : base(logger)
    {
        _dbContext = dbContext;
        _adminUserDirectory = adminUserDirectory;
        _initialAdminCreator = initialAdminCreator;
        _bootstrapSecretValidator = bootstrapSecretValidator;
    }

    /// <inheritdoc />
    public async Task<Result<BootstrapStatusDto>> GetBootstrapStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasAnyAdminUser = await _adminUserDirectory.HasAnyAdminUserAsync(cancellationToken);

            if (hasAnyAdminUser.IsFailed)
            {
                return hasAnyAdminUser.ToResult<BootstrapStatusDto>();
            }

            var utcNow = DateTime.UtcNow;
            await ExpireStaleSessions(utcNow, cancellationToken);
            var activeSession = await _dbContext.BootstrapSessions
                .AsNoTracking()
                .Where(session => session.Status != EBootstrapSessionStatus.Cancelled
                                  && session.Status != EBootstrapSessionStatus.Completed
                                  && session.Status != EBootstrapSessionStatus.Expired
                                  && session.ExpiresAtUtc > utcNow)
                .OrderBy(session => session.ExpiresAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            return new BootstrapStatusDto(
                hasAnyAdminUser.Value,
                activeSession is not null,
                activeSession?.ExpiresAtUtc);
        }
        catch (Exception ex)
        {
            return new ExternalError("Failed to get bootstrap status").CausedBy(ex);
        }
    }

    /// <summary>
    /// Starts the bootstrap process for the platform.
    /// This will allow the caller to create the first admin user for the system,
    /// which is required to complete the bootstrap process and configure the system for use.
    /// </summary>
    /// <param name="email">The email address of the first admin user in the system.</param>
    /// <param name="bootstrapSecret">The pre-defined secret, to authenticate the request.</param>
    /// <param name="createdFromIp">The remote IP address that started the bootstrap session, when available.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A session token for authenticating subsequent bootstrap requests.</returns>
    public async Task<Result<string>> StartBootstrap(
        string email,
        string bootstrapSecret,
        string? createdFromIp,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = NormalizeEmail(email);

            if (normalizedEmail is null)
            {
                return new InvalidBootstrapEmailError();
            }

            if (!_bootstrapSecretValidator.IsValid(bootstrapSecret))
            {
                Logger.LogWarning("Rejected bootstrap start because the supplied bootstrap secret was invalid.");
                return new InvalidBootstrapSecret();
            }

            var isBootstrapRequired = await GetBootstrapStatus(cancellationToken);

            if (isBootstrapRequired.IsFailed) return isBootstrapRequired.ToResult();

            if (isBootstrapRequired.Value.IsConfigured)
            {
                return new SystemAlreadyConfiguredError();
            }

            var utcNow = DateTime.UtcNow;
            await ExpireStaleSessions(utcNow, cancellationToken);
            var activeSession = await _dbContext.BootstrapSessions.FirstOrDefaultAsync(
                session => session.Status != EBootstrapSessionStatus.Cancelled
                           && session.Status != EBootstrapSessionStatus.Completed
                           && session.Status != EBootstrapSessionStatus.Expired
                           && session.ExpiresAtUtc > utcNow,
                cancellationToken);

            if (activeSession != null) return new ActiveBootstrapSessionError();

            var sessionToken = GenerateSessionToken();
            var session = new BootstrapSession
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Status = EBootstrapSessionStatus.Verified,
                BootstrapSecretHash = HashSecret(bootstrapSecret),
                SessionTokenHash = HashSecret(sessionToken),
                ExpiresAtUtc = utcNow.Add(SessionLifetime),
                VerifiedAtUtc = utcNow,
                CreatedAtUtc = utcNow,
                ActiveBootstrapSlot = 1,
                CreatedFromIp = string.IsNullOrWhiteSpace(createdFromIp) ? null : createdFromIp.Trim(),
                LastSeenAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            };

            _dbContext.BootstrapSessions.Add(session);
            await _dbContext.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Started bootstrap session {BootstrapSessionId}.", session.Id);
            return sessionToken;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogWarning(ex, "Rejected bootstrap start because another active bootstrap session was persisted first.");
            return new ActiveBootstrapSessionError();
        }
        catch (Exception ex)
        {
            return new ExternalError("Failed to start bootstrap").CausedBy(ex);
        }
    }

    /// <inheritdoc />
    public async Task<Result> CompleteBootstrap(
        string sessionToken,
        string password,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                return new InvalidBootstrapSessionError();
            }

            var isBootstrapRequired = await GetBootstrapStatus(cancellationToken);

            if (isBootstrapRequired.IsFailed) return isBootstrapRequired.ToResult();

            if (isBootstrapRequired.Value.IsConfigured)
            {
                return new SystemAlreadyConfiguredError();
            }

            var utcNow = DateTime.UtcNow;
            var sessionTokenHash = HashSecret(sessionToken);
            var session = await _dbContext.BootstrapSessions.FirstOrDefaultAsync(
                candidate => candidate.SessionTokenHash == sessionTokenHash,
                cancellationToken);

            if (session is null)
            {
                return new InvalidBootstrapSessionError();
            }

            if (session.ExpiresAtUtc <= utcNow)
            {
                session.Status = EBootstrapSessionStatus.Expired;
                session.ActiveBootstrapSlot = null;
                session.LastSeenAtUtc = utcNow;
                session.UpdatedAtUtc = utcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return new ExpiredBootstrapSessionError();
            }

            if (session.Status != EBootstrapSessionStatus.Verified)
            {
                return new BootstrapSessionNotVerifiedError();
            }

            var createAdminResult = await _initialAdminCreator.CreateInitialAdminUserAsync(
                session.Email,
                password,
                displayName,
                cancellationToken);

            if (createAdminResult.IsFailed)
            {
                return createAdminResult;
            }

            session.Status = EBootstrapSessionStatus.Completed;
            session.ActiveBootstrapSlot = null;
            session.CompletedAtUtc = utcNow;
            session.LastSeenAtUtc = utcNow;
            session.UpdatedAtUtc = utcNow;
            await SetBootstrapCompletedFlag(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Completed bootstrap session {BootstrapSessionId}.", session.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return new ExternalError("Failed to complete bootstrap").CausedBy(ex);
        }
    }

    /// <summary>
    /// Marks stale active bootstrap sessions expired before evaluating current state.
    /// </summary>
    /// <param name="utcNow">The current UTC timestamp.</param>
    /// <param name="cancellationToken">A token that cancels the database operation.</param>
    private async Task ExpireStaleSessions(DateTime utcNow, CancellationToken cancellationToken)
    {
        var staleSessions = await _dbContext.BootstrapSessions
            .Where(session => session.Status != EBootstrapSessionStatus.Cancelled
                              && session.Status != EBootstrapSessionStatus.Completed
                              && session.Status != EBootstrapSessionStatus.Expired
                              && session.ExpiresAtUtc <= utcNow)
            .ToListAsync(cancellationToken);

        foreach (var session in staleSessions)
        {
            session.Status = EBootstrapSessionStatus.Expired;
            session.ActiveBootstrapSlot = null;
            session.UpdatedAtUtc = utcNow;
        }

        if (staleSessions.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sets the secondary bootstrap-completed system flag after initial administrator creation succeeds.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the database operation.</param>
    private async Task SetBootstrapCompletedFlag(CancellationToken cancellationToken)
    {
        var flag = await _dbContext.SystemFlags.FindAsync([BootstrapCompletedFlagKey], cancellationToken);

        if (flag is null)
        {
            _dbContext.SystemFlags.Add(new SystemFlag
            {
                Key = BootstrapCompletedFlagKey,
                Value = true
            });
            return;
        }

        flag.Value = true;
    }

    /// <summary>
    /// Normalizes an email address before it is stored on the bootstrap session.
    /// </summary>
    /// <param name="email">The email address supplied during bootstrap start.</param>
    /// <returns>The normalized email address, or <see langword="null" /> when invalid.</returns>
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
    /// Generates a random bootstrap session token for storage in a secure HTTP-only cookie.
    /// </summary>
    /// <returns>A URL-safe random session token.</returns>
    private static string GenerateSessionToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>
    /// Hashes secret material before persistence or lookup.
    /// </summary>
    /// <param name="secret">The raw secret material.</param>
    /// <returns>A SHA-256 hash string with an algorithm prefix.</returns>
    private static string HashSecret(string secret)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return $"sha256.{Convert.ToBase64String(hash)}";
    }
}
