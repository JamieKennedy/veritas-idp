using FluentResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.Admin.API.Controllers.v1;
using Veritas.Admin.API.Models.Setup;
using Veritas.MessagingService.Application.Services;
using Veritas.PlatformService.Application.DataTransferObjects.Setup;
using Veritas.PlatformService.Application.Interfaces;

using Xunit;

namespace Veritas.Admin.API.Tests.Setup;

public sealed class SetupControllerTests
{
    [Fact]
    public async Task DeferSmtpSetupAsync_persists_the_authenticated_administrators_decision()
    {
        var bootstrapService = new StubBootstrapService();
        var controller = CreateController(bootstrapService);

        var result = await controller.DeferSmtpSetupAsync(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.True(bootstrapService.DeferSmtpSetupCalled);
    }

    [Fact]
    public async Task StatusAsync_returns_the_durable_smtp_deferral_state()
    {
        var bootstrapService = new StubBootstrapService
        {
            Status = new BootstrapStatusDto(true, false, null, true),
        };
        var controller = CreateController(bootstrapService);

        var result = await controller.StatusAsync(CancellationToken.None);

        var response = Assert.IsType<SetupStatusResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.True(response.IsSmtpSetupDeferred);
    }

    [Fact]
    public void DeferSmtpSetupAsync_requires_an_authenticated_administrator()
    {
        var method = typeof(SetupController).GetMethod(nameof(SetupController.DeferSmtpSetupAsync));

        Assert.NotNull(method);
        Assert.IsType<AuthorizeAttribute>(Assert.Single(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)));
    }

    private static SetupController CreateController(IBootstrapService bootstrapService)
    {
        return new SetupController(NullLogger<SetupController>.Instance, bootstrapService, new StubSmtpSetupStatus());
    }

    private sealed class StubBootstrapService : IBootstrapService
    {
        public BootstrapStatusDto Status { get; init; } = new(false, false, null, false);

        public bool DeferSmtpSetupCalled
        {
            get; private set;
        }

        public Task<Result<BootstrapStatusDto>> GetBootstrapStatusAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok(Status));
        }

        public Task<Result<string>> StartBootstrapAsync(
            string email,
            string bootstrapSecret,
            string? createdFromIp,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok(string.Empty));
        }

        public Task<Result> CompleteBootstrapAsync(
            string sessionToken,
            string password,
            string? displayName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }

        public Task<Result> DeferSmtpSetupAsync(CancellationToken cancellationToken = default)
        {
            DeferSmtpSetupCalled = true;
            return Task.FromResult(Result.Ok());
        }
    }

    private sealed class StubSmtpSetupStatus : ISmtpSetupStatus
    {
        public Task<bool> IsSmtpConfiguredAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }
}
