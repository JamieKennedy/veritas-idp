using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Veritas.PlatformService.Application.Dependencies;
using Veritas.PlatformService.Application.Services;
using Veritas.PlatformService.Infrastructure.Database;
using Xunit;

namespace Veritas.PlatformService.Application.Tests;

public sealed class BootstrapServiceTests
{
    [Fact]
    public async Task GetBootstrapStatus_uses_admin_user_directory_without_transport_dependency()
    {
        await using var context = CreateContext();
        var directory = new StubAdminUserDirectory(false);
        var service = new BootstrapService(NullLogger<BootstrapService>.Instance, context, directory);

        var result = await service.GetBootstrapStatus(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(1, directory.CallCount);
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new PlatformDbContext(options);
    }

    private sealed class StubAdminUserDirectory(bool hasAnyAdminUser) : IAdminUserDirectory
    {
        public int CallCount { get; private set; }

        public Task<Result<bool>> HasAnyAdminUserAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(Result.Ok(hasAnyAdminUser));
        }
    }
}
