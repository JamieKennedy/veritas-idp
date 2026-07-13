using FluentResults;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.PlatformService.Application.Dependencies;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Services;
using Veritas.PlatformService.Domain.Entities;
using Veritas.PlatformService.Domain.Types;
using Veritas.PlatformService.Infrastructure.Database;

using Xunit;

namespace Veritas.PlatformService.Application.Tests;

public sealed class BootstrapServiceTests
{
    [Fact]
    public void ConfiguredBootstrapSecretValidator_accepts_exact_configured_secret()
    {
        var validator = new ConfiguredBootstrapSecretValidator("expected-secret");

        Assert.True(validator.IsConfigured);
        Assert.True(validator.IsValid("expected-secret"));
    }

    [Fact]
    public void ConfiguredBootstrapSecretValidator_rejects_wrong_secret()
    {
        var validator = new ConfiguredBootstrapSecretValidator("expected-secret");

        Assert.False(validator.IsValid("wrong-secret"));
    }

    [Fact]
    public async Task GetBootstrapStatus_uses_admin_user_directory_without_transport_dependency()
    {
        await using var context = CreateContext();
        var directory = new StubAdminUserDirectory(false);
        var adminCreator = new StubInitialAdminCreator();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            directory,
            adminCreator,
            new FixedBootstrapSecretValidator("expected-secret"));

        var result = await service.GetBootstrapStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsConfigured);
        Assert.False(result.Value.HasActiveBootstrap);
        Assert.Equal(1, directory.CallCount);
    }

    [Fact]
    public async Task GetBootstrapStatus_reports_active_bootstrap_expiry()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));
        var start = await service.StartBootstrapAsync(
            "admin@example.com",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        var result = await service.GetBootstrapStatusAsync(CancellationToken.None);

        Assert.True(start.IsSuccess);
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsConfigured);
        Assert.True(result.Value.HasActiveBootstrap);
        Assert.NotNull(result.Value.ActiveBootstrapExpiresAtUtc);
    }

    [Fact]
    public async Task DeferSmtpSetup_persists_an_idempotent_platform_setup_flag()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(true),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));

        var firstResult = await service.DeferSmtpSetupAsync(CancellationToken.None);
        var secondResult = await service.DeferSmtpSetupAsync(CancellationToken.None);
        var status = await service.GetBootstrapStatusAsync(CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.True(status.IsSuccess);
        Assert.True(status.Value.IsSmtpSetupDeferred);
        var deferredFlag = await context.SystemFlags.FindAsync("SMTP_SETUP_DEFERRED");
        Assert.NotNull(deferredFlag);
        Assert.True(deferredFlag.Value);
        Assert.Single(context.SystemFlags, flag => flag.Key == "SMTP_SETUP_DEFERRED");
    }

    [Fact]
    public async Task StartBootstrap_creates_verified_no_email_session_and_returns_raw_session_token()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));

        var result = await service.StartBootstrapAsync(
            "  ADMIN@Example.COM ",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Value));
        var session = Assert.Single(context.BootstrapSessions);
        Assert.Equal("admin@example.com", session.Email);
        Assert.Equal(BootstrapSessionStatus.Verified, session.Status);
        Assert.NotEqual(result.Value, session.SessionTokenHash);
        Assert.StartsWith("sha256.", session.SessionTokenHash, StringComparison.Ordinal);
        Assert.NotNull(session.VerifiedAtUtc);
        Assert.Equal(1, session.ActiveBootstrapSlot);
        Assert.Equal("203.0.113.10", session.CreatedFromIp);
        Assert.NotNull(session.LastSeenAtUtc);
    }

    [Fact]
    public void PlatformDbContext_model_has_unique_active_bootstrap_slot()
    {
        using var context = CreateContext();
        var bootstrapSessionType = context.Model.FindEntityType(typeof(BootstrapSession));
        var activeBootstrapSlot = bootstrapSessionType?.FindProperty("ActiveBootstrapSlot");
        var activeBootstrapIndex = bootstrapSessionType?.GetIndexes()
            .SingleOrDefault(index => index.Properties.Count == 1 && index.Properties[0].Name == "ActiveBootstrapSlot");

        Assert.NotNull(activeBootstrapSlot);
        Assert.NotNull(activeBootstrapIndex);
        Assert.True(activeBootstrapIndex.IsUnique);
    }

    [Fact]
    public async Task StartBootstrap_rejects_invalid_bootstrap_secret_without_creating_session()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));

        var result = await service.StartBootstrapAsync(
            "admin@example.com",
            "wrong-secret",
            "203.0.113.10",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(context.BootstrapSessions);
    }

    [Fact]
    public async Task StartBootstrap_rejects_display_name_email_syntax()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));

        var result = await service.StartBootstrapAsync(
            "First Admin <admin@example.com>",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(context.BootstrapSessions);
    }

    [Fact]
    public async Task StartBootstrap_rejects_second_active_session()
    {
        await using var context = CreateContext();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            new StubInitialAdminCreator(),
            new FixedBootstrapSecretValidator("expected-secret"));
        await service.StartBootstrapAsync(
            "admin@example.com",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        var result = await service.StartBootstrapAsync(
            "other@example.com",
            "expected-secret",
            "203.0.113.11",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Single(context.BootstrapSessions);
    }

    [Fact]
    public async Task CompleteBootstrap_uses_session_email_to_create_initial_admin_and_marks_session_completed()
    {
        await using var context = CreateContext();
        var adminCreator = new StubInitialAdminCreator();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            adminCreator,
            new FixedBootstrapSecretValidator("expected-secret"));
        var start = await service.StartBootstrapAsync(
            "admin@example.com",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        var result = await service.CompleteBootstrapAsync(
            start.Value,
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("admin@example.com", adminCreator.Email);
        Assert.Equal("Correct Horse Battery Staple 42!", adminCreator.Password);
        Assert.Equal("First Admin", adminCreator.DisplayName);
        var session = Assert.Single(context.BootstrapSessions);
        Assert.Equal(BootstrapSessionStatus.Completed, session.Status);
        Assert.Null(session.ActiveBootstrapSlot);
        Assert.NotNull(session.CompletedAtUtc);
        Assert.NotNull(session.LastSeenAtUtc);
        var completedFlag = await context.SystemFlags.FindAsync("BOOTSTRAP_COMPLETED");
        Assert.NotNull(completedFlag);
        Assert.True(completedFlag.Value);
    }

    [Fact]
    public async Task CompleteBootstrap_marks_expired_session_and_does_not_create_admin()
    {
        await using var context = CreateContext();
        var adminCreator = new StubInitialAdminCreator();
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            adminCreator,
            new FixedBootstrapSecretValidator("expected-secret"));
        var start = await service.StartBootstrapAsync(
            "admin@example.com",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);
        var session = Assert.Single(context.BootstrapSessions);
        session.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();

        var result = await service.CompleteBootstrapAsync(
            start.Value,
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Null(adminCreator.Email);
        Assert.Equal(BootstrapSessionStatus.Expired, session.Status);
        Assert.Null(session.ActiveBootstrapSlot);
    }

    [Fact]
    public async Task CompleteBootstrap_leaves_session_verified_when_initial_admin_creation_fails()
    {
        await using var context = CreateContext();
        var adminCreator = new StubInitialAdminCreator(Result.Fail("Users unavailable"));
        var service = new BootstrapService(
            NullLogger<BootstrapService>.Instance,
            context,
            new StubAdminUserDirectory(false),
            adminCreator,
            new FixedBootstrapSecretValidator("expected-secret"));
        var start = await service.StartBootstrapAsync(
            "admin@example.com",
            "expected-secret",
            "203.0.113.10",
            CancellationToken.None);

        var result = await service.CompleteBootstrapAsync(
            start.Value,
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        var session = Assert.Single(context.BootstrapSessions);
        Assert.Equal(BootstrapSessionStatus.Verified, session.Status);
        Assert.Empty(context.SystemFlags);
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
        public int CallCount
        {
            get; private set;
        }

        public Task<Result<bool>> HasAnyAdminUserAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(Result.Ok(hasAnyAdminUser));
        }
    }

    private sealed class StubInitialAdminCreator(Result? result = null) : IInitialAdminCreator
    {
        public string? Email
        {
            get; private set;
        }
        public string? Password
        {
            get; private set;
        }
        public string? DisplayName
        {
            get; private set;
        }

        public Task<Result> CreateInitialAdminUserAsync(
            string email,
            string password,
            string? displayName,
            CancellationToken cancellationToken = default)
        {
            Email = email;
            Password = password;
            DisplayName = displayName;
            return Task.FromResult(result ?? Result.Ok());
        }
    }

    private sealed class FixedBootstrapSecretValidator(string expectedSecret) : IBootstrapSecretValidator
    {
        public bool IsConfigured => true;

        public bool IsValid(string candidateSecret)
        {
            return candidateSecret == expectedSecret;
        }
    }
}
