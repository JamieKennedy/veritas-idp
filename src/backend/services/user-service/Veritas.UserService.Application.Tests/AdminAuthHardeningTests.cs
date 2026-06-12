using System.Globalization;
using System.Security.Cryptography;
using System.Text;
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
        Assert.Equal(EAdminLoginChallengePurpose.MfaEnrollment, result.Value.Purpose);
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

    private static AdminUserService CreateService(UserDbContext context)
    {
        return new AdminUserService(
            NullLogger<AdminUserService>.Instance,
            context,
            new PassThroughSecretProtector(),
            TimeProvider.System);
    }

    private static async Task<AdminMfaEnrollmentResultDto> EnrollMfaAsync(AdminUserService service)
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
        var totpCode = TotpTestCodeGenerator.Generate(challenge.Value.TotpSecretBase32!, DateTimeOffset.UtcNow);
        var enrollment = await service.CompleteAdminMfaEnrollmentAsync(
            challenge.Value.ChallengeId,
            challenge.Value.ChallengeToken,
            totpCode,
            CancellationToken.None);

        return enrollment.Value;
    }

    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
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

            using var hmac = new HMACSHA1(key);
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

            return bytes.ToArray();
        }
    }
}
