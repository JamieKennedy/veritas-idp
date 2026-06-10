using System.Reflection;
using Veritas.MessagingService.Domain.Errors;
using Veritas.PlatformService.Domain.Errors.Bootstrap;
using Veritas.PlatformService.Domain.Errors.Base;
using Veritas.Shared.Errors;
using Veritas.UserService.Domain.Errors.AdminUsers;
using Xunit;

namespace Veritas.Admin.API.Tests.Errors;

public sealed class DomainErrorCodeTests
{
    [Fact]
    public void Domain_error_models_have_unique_stable_codes()
    {
        var errorTypes = new[]
            {
                typeof(ActiveBootstrapSessionError).Assembly,
                typeof(SmtpNotConfiguredError).Assembly,
                typeof(InvalidAdminCredentialsError).Assembly
            }
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false } && typeof(VeritasError).IsAssignableFrom(type))
            .ToList();

        var errorCodes = errorTypes.Select(type => new
            {
                Type = type,
                Code = type.GetField("ErrorCode", BindingFlags.Public | BindingFlags.Static)?.GetRawConstantValue() as string
            })
            .ToList();
        var duplicateCodes = errorCodes
            .Where(item => !string.IsNullOrWhiteSpace(item.Code))
            .GroupBy(item => item.Code)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.All(errorCodes, item => Assert.False(string.IsNullOrWhiteSpace(item.Code), $"{item.Type.FullName} must define a public const string ErrorCode."));
        Assert.Empty(duplicateCodes);
    }

    [Fact]
    public void Domain_error_models_attach_unique_code_metadata()
    {
        var error = new InvalidAdminCredentialsError();

        Assert.Equal(InvalidAdminCredentialsError.ErrorCode, error.Metadata[ErrorMetadataKeys.Code]);
    }
}
