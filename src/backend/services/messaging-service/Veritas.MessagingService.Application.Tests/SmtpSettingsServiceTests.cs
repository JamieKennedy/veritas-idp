using FluentResults;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Domain.Types;
using Veritas.MessagingService.Infrastructure.Database;
using Veritas.Shared.Security;

using Xunit;

namespace Veritas.MessagingService.Application.Tests;

public sealed class SmtpSettingsServiceTests
{
    [Fact]
    public async Task ConfigureSmtpAsync_rejects_invalid_host_with_stable_error_code()
    {
        await using var context = CreateContext();
        var service = new SmtpSettingsService(
            NullLogger<SmtpSettingsService>.Instance,
            context,
            new RecordingSecretProtector(),
            new StubSmtpConnectivityTester(Result.Ok()));

        var result = await service.ConfigureSmtpAsync(
            new ConfigureSmtpSettingsDto(
                " ",
                1025,
                SmtpTlsMode.None,
                null,
                null,
                "noreply@example.com",
                "Veritas"),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.IsType<InvalidSmtpSettingsError>(error);
        Assert.Equal(InvalidSmtpSettingsError.ErrorCode, error.Metadata["Code"]);
        Assert.Empty(context.SmtpSettings);
    }

    [Fact]
    public async Task ConfigureSmtpAsync_does_not_persist_settings_when_test_send_fails()
    {
        await using var context = CreateContext();
        var service = new SmtpSettingsService(
            NullLogger<SmtpSettingsService>.Instance,
            context,
            new RecordingSecretProtector(),
            new StubSmtpConnectivityTester(Result.Fail(new SmtpConnectivityFailedError())));

        var result = await service.ConfigureSmtpAsync(ValidSettings(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<SmtpConnectivityFailedError>(Assert.Single(result.Errors));
        Assert.Empty(context.SmtpSettings);
    }

    [Fact]
    public async Task ConfigureSmtpAsync_persists_encrypted_settings_after_successful_test_send()
    {
        await using var context = CreateContext();
        var protector = new RecordingSecretProtector();
        var tester = new StubSmtpConnectivityTester(Result.Ok());
        var service = new SmtpSettingsService(
            NullLogger<SmtpSettingsService>.Instance,
            context,
            protector,
            tester);

        var result = await service.ConfigureSmtpAsync(ValidSettings(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var settings = Assert.Single(context.SmtpSettings);
        Assert.Equal("smtp.example.com", settings.Host);
        Assert.Equal("protected:super-secret", settings.ProtectedSecret);
        Assert.NotEqual("super-secret", settings.ProtectedSecret);
        Assert.True(settings.IsConfigured);
        Assert.NotNull(settings.LastSuccessfulTestAtUtc);
        Assert.Equal("super-secret", tester.LastRequest?.Secret);
        Assert.Equal("super-secret", protector.LastProtectedPlaintext);
    }

    [Fact]
    public async Task IsSmtpConfiguredAsync_returns_true_only_after_successful_configuration()
    {
        await using var context = CreateContext();
        var service = new SmtpSettingsService(
            NullLogger<SmtpSettingsService>.Instance,
            context,
            new RecordingSecretProtector(),
            new StubSmtpConnectivityTester(Result.Ok()));

        Assert.False(await service.IsSmtpConfiguredAsync(CancellationToken.None));

        var result = await service.ConfigureSmtpAsync(ValidSettings(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(await service.IsSmtpConfiguredAsync(CancellationToken.None));
    }

    private static ConfigureSmtpSettingsDto ValidSettings()
    {
        return new ConfigureSmtpSettingsDto(
            "smtp.example.com",
            1025,
            SmtpTlsMode.None,
            "smtp-user",
            "super-secret",
            "noreply@example.com",
            "Veritas");
    }

    private static MessagingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MessagingDbContext(options);
    }

    private sealed class RecordingSecretProtector : ISecretProtector
    {
        public string? LastProtectedPlaintext
        {
            get; private set;
        }

        public string Protect(string plaintext)
        {
            LastProtectedPlaintext = plaintext;
            return $"protected:{plaintext}";
        }

        public string Unprotect(string protectedText)
        {
            return protectedText.StartsWith("protected:", StringComparison.Ordinal)
                ? protectedText["protected:".Length..]
                : protectedText;
        }
    }

    private sealed class StubSmtpConnectivityTester(Result result) : ISmtpConnectivityTester
    {
        public SmtpConnectivityTestRequest? LastRequest
        {
            get; private set;
        }

        public Task<Result> TestAsync(SmtpConnectivityTestRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(result);
        }
    }
}
