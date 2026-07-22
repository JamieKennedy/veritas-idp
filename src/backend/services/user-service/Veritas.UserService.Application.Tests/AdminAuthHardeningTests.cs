using System.Globalization;
using System.Security.Cryptography;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.Shared.Security;
using Veritas.UserService.Application.DataTransferObjects;
using Veritas.UserService.Application.Services;
using Veritas.UserService.Domain.Entities;
using Veritas.UserService.Infrastructure.Database;

using Xunit;

namespace Veritas.UserService.Application.Tests;

public sealed class AdminAuthHardeningTests
{
    [Fact]
    public async Task StartAdminLoginAsync_requires_mfa_enrollment_for_admin_without_mfa()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        var result = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AdminLoginChallengePurpose.MfaEnrollment, result.Value.Purpose);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.ChallengeToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.TotpSecretBase32));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.TotpProvisioningUri));
        Assert.Empty(context.AdminSessions);
        Assert.Single(context.AdminLoginChallenges);
    }

    [Fact]
    public async Task CompleteAdminMfaEnrollmentAsync_enables_mfa_returns_recovery_codes_and_creates_session()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var totpCode = TotpTestCodeGenerator.Generate(challenge.Value.TotpSecretBase32!, DateTimeOffset.UtcNow);

        var result = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("admin@example.com", result.Value.Admin.Email);
        Assert.NotEqual(Guid.Empty, result.Value.SessionId);
        Assert.Equal(10, result.Value.RecoveryCodes.Count);
        Assert.All(result.Value.RecoveryCodes, code => Assert.StartsWith("veritas-", code, StringComparison.Ordinal));
        var adminUser = Assert.Single(context.AdminUsers);
        Assert.NotNull(adminUser.MfaEnabledAtUtc);
        Assert.NotNull(adminUser.MfaSecretProtected);
        Assert.NotEqual(Guid.Empty, adminUser.SecurityStamp);
        Assert.Single(context.AdminSessions);
        Assert.Equal(10, context.AdminRecoveryCodes.Count());
        Assert.NotEqual(result.Value.RecoveryCodes[0], context.AdminRecoveryCodes.First().CodeHash);
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_accepts_totp_for_mfa_enabled_admin_and_creates_session()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await EnrollMfaAsync(service);

        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var secret = new PassThroughSecretProtector().Unprotect(context.AdminUsers.Single().MfaSecretProtected!);
        var totpCode = TotpTestCodeGenerator.Generate(secret, DateTimeOffset.UtcNow);

        var result = await service.CompleteAdminMfaVerificationAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.SessionId);
        Assert.Equal(2, context.AdminSessions.Count());
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_consumes_recovery_code_once()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var enrollment = await EnrollMfaAsync(service);
        var recoveryCode = enrollment.RecoveryCodes[0];
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);

        var result = await service.CompleteAdminMfaVerificationAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            recoveryCode,
            CancellationToken.None);
        var secondChallenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var reuseResult = await service.CompleteAdminMfaVerificationAsync(
            secondChallenge.Value.ChallengeId,
            secondChallenge.Value.ChallengeToken,
            recoveryCode,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(reuseResult.IsFailed);
        Assert.Single(context.AdminRecoveryCodes.Where(code => code.ConsumedAtUtc != null));
    }

    [Fact]
    public async Task ValidateAdminSessionAsync_rejects_revoked_session()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        var enrollment = await EnrollMfaAsync(service);

        var revokeResult = await service.RevokeAdminSessionAsync(
            enrollment.Admin.Id,
            enrollment.SessionId,
            "logout",
            CancellationToken.None);
        var validateResult = await service.ValidateAdminSessionAsync(
            enrollment.Admin.Id,
            enrollment.SessionId,
            enrollment.SecurityStamp,
            CancellationToken.None);

        Assert.True(revokeResult.IsSuccess);
        Assert.True(validateResult.IsFailed);
    }

    [Fact]
    public async Task CompleteAdminMfaEnrollmentAsync_rejects_replayed_challenge()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        await using var context = CreateContext();
        var service = CreateService(context, timeProvider);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var totpCode = TotpTestCodeGenerator.Generate(challenge.Value.TotpSecretBase32!, timeProvider.GetUtcNow());

        var firstResult = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);
        var replayResult = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(replayResult.IsFailed);
        Assert.Single(context.AdminSessions);
        Assert.Equal(10, context.AdminRecoveryCodes.Count());
    }

    [Fact]
    public async Task CompleteAdminMfaEnrollmentAsync_rejects_expired_challenge()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        await using var context = CreateContext();
        var service = CreateService(context, timeProvider);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(6));
        var totpCode = TotpTestCodeGenerator.Generate(challenge.Value.TotpSecretBase32!, timeProvider.GetUtcNow());

        var result = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(context.AdminSessions);
        Assert.Empty(context.AdminRecoveryCodes);
        Assert.Null(context.AdminUsers.Single().MfaEnabledAtUtc);
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_rejects_replayed_challenge()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        await using var context = CreateContext();
        var service = CreateService(context, timeProvider);
        await EnrollMfaAsync(service, timeProvider);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var secret = new PassThroughSecretProtector().Unprotect(context.AdminUsers.Single().MfaSecretProtected!);
        var totpCode = TotpTestCodeGenerator.Generate(secret, timeProvider.GetUtcNow());

        var firstResult = await service.CompleteAdminMfaVerificationAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);
        var replayResult = await service.CompleteAdminMfaVerificationAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(replayResult.IsFailed);
        Assert.Equal(2, context.AdminSessions.Count());
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_does_not_consume_recovery_code_on_invalid_attempt()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        await EnrollMfaAsync(service);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);

        var result = await service.CompleteAdminMfaVerificationAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            "veritas-invalid-recovery-code",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.DoesNotContain(context.AdminRecoveryCodes, code => code.ConsumedAtUtc is not null);
        Assert.Null(context.AdminLoginChallenges.Single(candidate => candidate.Id == challenge.Value.ChallengeId).ConsumedAtUtc);
    }

    [Fact]
    public async Task ValidateAdminSessionAsync_rejects_idle_expired_session()
    {
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        await using var context = CreateContext();
        var service = CreateService(context, timeProvider);
        var enrollment = await EnrollMfaAsync(service, timeProvider);
        timeProvider.Advance(TimeSpan.FromMinutes(31));

        var result = await service.ValidateAdminSessionAsync(
            enrollment.Admin.Id,
            enrollment.SessionId,
            enrollment.SecurityStamp,
            CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task StartAdminLoginAsync_propagates_caller_cancellation()
    {
        await using var context = CreateContext();
        var service = CreateService(context);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_allows_only_one_concurrent_challenge_replay()
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared;Default Timeout=10";
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        AdminLoginChallengeDto challenge;
        string totpCode;

        await using var databaseAnchor = new SqliteConnection(connectionString);
        await databaseAnchor.OpenAsync();
        await using (var seedContext = CreateSqliteContext(connectionString))
        {
            await seedContext.Database.EnsureCreatedAsync();
            var seedService = CreateService(seedContext, timeProvider);
            await EnrollMfaAsync(seedService, timeProvider);
            var challengeResult = await seedService.StartAdminLoginAsync(
                "admin@example.com",
                "Correct Horse Battery Staple 42!",
                CancellationToken.None);
            challenge = challengeResult.Value;
            var secret = new PassThroughSecretProtector().Unprotect(seedContext.AdminUsers.Single().MfaSecretProtected!);
            totpCode = TotpTestCodeGenerator.Generate(secret, timeProvider.GetUtcNow());
        }

        await using var firstContext = CreateSqliteContext(connectionString);
        await using var secondContext = CreateSqliteContext(connectionString);
        using var barrier = new Barrier(2);
        var firstService = CreateService(firstContext, timeProvider, new CoordinatedSecretProtector(barrier));
        var secondService = CreateService(secondContext, timeProvider, new CoordinatedSecretProtector(barrier));

        var results = await Task.WhenAll(
            Task.Run(() => firstService.CompleteAdminMfaVerificationAsync(
                challenge.ChallengeId,
                challenge.ChallengeToken,
                totpCode,
                CancellationToken.None)),
            Task.Run(() => secondService.CompleteAdminMfaVerificationAsync(
                challenge.ChallengeId,
                challenge.ChallengeToken,
                totpCode,
                CancellationToken.None)));

        Assert.Single(results, result => result.IsSuccess);
        Assert.Single(results, result => result.IsFailed);
        await using var assertionContext = CreateSqliteContext(connectionString);
        Assert.Equal(2, assertionContext.AdminSessions.Count());
        Assert.NotNull(assertionContext.AdminLoginChallenges.Single(candidate => candidate.Id == challenge.ChallengeId).ConsumedAtUtc);
    }

    [Fact]
    public async Task CompleteAdminMfaVerificationAsync_consumes_recovery_code_once_under_concurrency()
    {
        var connectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared;Default Timeout=10";
        var timeProvider = new MutableTimeProvider(new DateTimeOffset(2026, 7, 21, 12, 0, 0, TimeSpan.Zero));
        AdminLoginChallengeDto firstChallenge;
        AdminLoginChallengeDto secondChallenge;
        string recoveryCode;

        await using var databaseAnchor = new SqliteConnection(connectionString);
        await databaseAnchor.OpenAsync();
        await using (var seedContext = CreateSqliteContext(connectionString))
        {
            await seedContext.Database.EnsureCreatedAsync();
            var seedService = CreateService(seedContext, timeProvider);
            var enrollment = await EnrollMfaAsync(seedService, timeProvider);
            recoveryCode = enrollment.RecoveryCodes[0];
            firstChallenge = (await seedService.StartAdminLoginAsync(
                "admin@example.com",
                "Correct Horse Battery Staple 42!",
                CancellationToken.None)).Value;
            secondChallenge = (await seedService.StartAdminLoginAsync(
                "admin@example.com",
                "Correct Horse Battery Staple 42!",
                CancellationToken.None)).Value;
        }

        await using var firstContext = CreateSqliteContext(connectionString);
        await using var secondContext = CreateSqliteContext(connectionString);
        using var barrier = new Barrier(2);
        var firstService = CreateService(firstContext, timeProvider, new CoordinatedSecretProtector(barrier));
        var secondService = CreateService(secondContext, timeProvider, new CoordinatedSecretProtector(barrier));

        var results = await Task.WhenAll(
            Task.Run(() => firstService.CompleteAdminMfaVerificationAsync(
                firstChallenge.ChallengeId,
                firstChallenge.ChallengeToken,
                recoveryCode,
                CancellationToken.None)),
            Task.Run(() => secondService.CompleteAdminMfaVerificationAsync(
                secondChallenge.ChallengeId,
                secondChallenge.ChallengeToken,
                recoveryCode,
                CancellationToken.None)));

        Assert.Single(results, result => result.IsSuccess);
        Assert.Single(results, result => result.IsFailed);
        await using var assertionContext = CreateSqliteContext(connectionString);
        Assert.Single(assertionContext.AdminRecoveryCodes, code => code.ConsumedAtUtc is not null);
        Assert.Equal(2, assertionContext.AdminSessions.Count());
    }

    private static AdminUserService CreateService(
        UserDbContext context,
        TimeProvider? timeProvider = null,
        ISecretProtector? secretProtector = null)
    {
        return new AdminUserService(
            NullLogger<AdminUserService>.Instance,
            context,
            secretProtector ?? new PassThroughSecretProtector(),
            timeProvider ?? TimeProvider.System);
    }

    private static async Task<AdminMfaEnrollmentResultDto> EnrollMfaAsync(AdminUserService service, TimeProvider? timeProvider = null)
    {
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);
        var challenge = await service.StartAdminLoginAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);
        var totpCode = TotpTestCodeGenerator.Generate(
            challenge.Value.TotpSecretBase32!,
            (timeProvider ?? TimeProvider.System).GetUtcNow());
        var enrollment = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        return enrollment.Value;
    }

    private sealed class MutableTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
        }
    }

    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new UserDbContext(options);
    }

    private static UserDbContext CreateSqliteContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new UserDbContext(options);
    }

    private sealed class PassThroughSecretProtector : ISecretProtector
    {
        public string Protect(string plaintext)
        {
            return plaintext;
        }

        public string Unprotect(string protectedText)
        {
            return protectedText;
        }
    }

    private sealed class CoordinatedSecretProtector(Barrier barrier) : ISecretProtector
    {
        public string Protect(string plaintext)
        {
            return plaintext;
        }

        public string Unprotect(string protectedText)
        {
            if (!barrier.SignalAndWait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Concurrent MFA verification did not reach the coordination barrier.");
            }

            return protectedText;
        }
    }

    private static class TotpTestCodeGenerator
    {
        public static string Generate(string secretBase32, DateTimeOffset timestamp)
        {
            var key = DecodeBase32(secretBase32);
            var counter = timestamp.ToUnixTimeSeconds() / 30;
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

#pragma warning disable CA5350 // The test must generate RFC 6238-compatible HMAC-SHA1 codes used by authenticator applications.
            using var hmac = new HMACSHA1(key);
#pragma warning restore CA5350
            var hash = hmac.ComputeHash(counterBytes);
            var offset = hash[^1] & 0x0F;
            var binary =
                ((hash[offset] & 0x7F) << 24) |
                ((hash[offset + 1] & 0xFF) << 16) |
                ((hash[offset + 2] & 0xFF) << 8) |
                (hash[offset + 3] & 0xFF);
            var code = binary % 1_000_000;

            return code.ToString("D6", CultureInfo.InvariantCulture);
        }

        private static byte[] DecodeBase32(string value)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bits = 0;
            var bitBuffer = 0;
            var bytes = new List<byte>();

            foreach (var character in value.TrimEnd('=').ToUpperInvariant())
            {
                var index = alphabet.IndexOf(character, StringComparison.Ordinal);
                if (index < 0)
                {
                    throw new FormatException("Invalid Base32 character.");
                }

                bitBuffer = (bitBuffer << 5) | index;
                bits += 5;

                if (bits < 8)
                {
                    continue;
                }

                bytes.Add((byte)((bitBuffer >> (bits - 8)) & 0xFF));
                bits -= 8;
            }

            return [.. bytes];
        }
    }
}
