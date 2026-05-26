using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Veritas.UserService.Application.Services;
using Veritas.UserService.Domain.Entities;
using Veritas.UserService.Infrastructure.Database;
using Xunit;

namespace Veritas.UserService.Application.Tests;

public sealed class AdminUserServiceTests
{
    [Fact]
    public async Task GetAdminUserCountAsync_reads_from_module_db_context()
    {
        await using var context = CreateContext();
        context.AdminUsers.Add(new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            PasswordHash = "hashed-secret",
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.GetAdminUserCountAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
    }

    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new UserDbContext(options);
    }
}
