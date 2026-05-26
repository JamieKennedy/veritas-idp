using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.PlatformService.Application.Dependencies;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Persistence;
using Veritas.PlatformService.Domain.Errors.Base;
using Veritas.PlatformService.Domain.Errors.Bootstrap;
using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Application.Services;

public class BootstrapService : BaseService<BootstrapService>, IBootstrapService
{
    private readonly IPlatformDbContext _dbContext;
    private readonly IAdminUserDirectory _adminUserDirectory;

    public BootstrapService(
        ILogger<BootstrapService> logger,
        IPlatformDbContext dbContext,
        IAdminUserDirectory adminUserDirectory) : base(logger)
    {
        _dbContext = dbContext;
        _adminUserDirectory = adminUserDirectory;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> GetBootstrapStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasAnyAdminUser = await _adminUserDirectory.HasAnyAdminUserAsync(cancellationToken);

            if (hasAnyAdminUser.IsFailed)
            {
                return hasAnyAdminUser.ToResult<bool>();
            }

            // Bootstrap is required until at least one admin user exists.
            return !hasAnyAdminUser.Value;
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
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A session token for authenticating subsequent bootstrap requests.</returns>
    public async Task<Result<string>> StartBootstrap(
        string email,
        string bootstrapSecret,
        CancellationToken cancellationToken = default)
    {
        var isBootstrapRequired = await GetBootstrapStatus(cancellationToken);

        if (isBootstrapRequired.IsFailed) return isBootstrapRequired.ToResult();

        if (!isBootstrapRequired.Value)
        {
            return new SystemAlreadyConfiguredError();
        }

        var utcNow = DateTime.UtcNow;
        var activeSession = await _dbContext.BootstrapSessions.FirstOrDefaultAsync(
            session => session.Status != EBootstrapSessionStatus.Cancelled
                       && session.Status != EBootstrapSessionStatus.Completed
                       && session.ExpiresAtUtc > utcNow,
            cancellationToken);

        if (activeSession != null) return new ActiveBootstrapSessionError();

        // Bootstrap secret validation and session-token creation will be implemented with the bootstrap flow.
        return string.Empty;
    }
}
