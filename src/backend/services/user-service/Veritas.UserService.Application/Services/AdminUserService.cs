using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.UserService.Application.Interfaces;
using Veritas.UserService.Application.Persistence;

namespace Veritas.UserService.Application.Services;

public sealed class AdminUserService(
    ILogger<AdminUserService> logger,
    IUserDbContext dbContext) : BaseService<AdminUserService>(logger), IAdminUserService
{
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
            return Result.Fail<int>("Failed to get admin user count.");
        }
    }
}
