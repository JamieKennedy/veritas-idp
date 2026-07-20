using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.Shared.Errors;
using Veritas.UserService.Application.Services;
using Veritas.UserService.Domain.Entities;
using Veritas.UserService.Domain.Errors.AdminUsers;
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

    [Fact]
    public async Task CreateInitialAdminUserAsync_normalizes_email_and_stores_password_hash()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.CreateInitialAdminUserAsync(
            "  ADMIN@Example.COM  ",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var adminUser = Assert.Single(context.AdminUsers);
        Assert.Equal("admin@example.com", adminUser.Email);
        Assert.Equal("First Admin", adminUser.Name);
        Assert.NotEqual("Correct Horse Battery Staple 42!", adminUser.PasswordHash);
        Assert.StartsWith("v1.", adminUser.PasswordHash, StringComparison.Ordinal);
        Assert.NotEqual(Guid.Empty, adminUser.Id);
        Assert.True(adminUser.CreatedAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_marks_initial_admin_with_singleton_slot()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var adminUser = Assert.Single(context.AdminUsers);
        Assert.Equal(1, adminUser.InitialAdminSlot);
    }

    [Fact]
    public void UserDbContext_model_has_unique_initial_admin_slot()
    {
        using var context = CreateContext();
        var adminUserType = context.Model.FindEntityType(typeof(AdminUser));
        var initialAdminSlot = adminUserType?.FindProperty("InitialAdminSlot");
        var initialAdminIndex = adminUserType?.GetIndexes()
            .SingleOrDefault(index => index.Properties.Count == 1 && index.Properties[0].Name == "InitialAdminSlot");

        Assert.NotNull(initialAdminSlot);
        Assert.NotNull(initialAdminIndex);
        Assert.True(initialAdminIndex.IsUnique);
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_rejects_second_admin_user()
    {
        await using var context = CreateContext();
        context.AdminUsers.Add(new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            PasswordHash = "v1.existing",
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.CreateInitialAdminUserAsync(
            "new@example.com",
            "Correct Horse Battery Staple 42!",
            null,
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(1, await context.AdminUsers.CountAsync());
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_rejects_display_name_email_syntax()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.CreateInitialAdminUserAsync(
            "First Admin <admin@example.com>",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(context.AdminUsers);
    }

    [Fact]
    public async Task CreateInitialAdminUserAsync_tags_short_password_as_validation_failure()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);

        var result = await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "too-short",
            "First Admin",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.IsType<AdminPasswordTooShortError>(error);
        Assert.Equal(ErrorCategories.Validation, error.Metadata[ErrorMetadataKeys.Category]);
        Assert.Equal(AdminPasswordTooShortError.ErrorCode, error.Metadata[ErrorMetadataKeys.Code]);
        Assert.Equal("The administrator password must be at least 12 characters long.", error.Message);
    }

    [Fact]
    public async Task ValidateAdminCredentialsAsync_accepts_correct_password_for_normalized_email()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        var result = await service.ValidateAdminCredentialsAsync(
            "  ADMIN@Example.COM  ",
            "Correct Horse Battery Staple 42!",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("admin@example.com", result.Value.Email);
        Assert.Equal("First Admin", result.Value.Name);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task ValidateAdminCredentialsAsync_rejects_wrong_password()
    {
        await using var context = CreateContext();
        var service = new AdminUserService(NullLogger<AdminUserService>.Instance, context);
        await service.CreateInitialAdminUserAsync(
            "admin@example.com",
            "Correct Horse Battery Staple 42!",
            "First Admin",
            CancellationToken.None);

        var result = await service.ValidateAdminCredentialsAsync(
            "admin@example.com",
            "wrong password",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        var error = Assert.Single(result.Errors);
        Assert.IsType<InvalidAdminCredentialsError>(error);
        Assert.Equal(ErrorCategories.Unauthorized, error.Metadata[ErrorMetadataKeys.Category]);
        Assert.Equal(InvalidAdminCredentialsError.ErrorCode, error.Metadata[ErrorMetadataKeys.Code]);
    }

    private static UserDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new UserDbContext(options);
    }
}
