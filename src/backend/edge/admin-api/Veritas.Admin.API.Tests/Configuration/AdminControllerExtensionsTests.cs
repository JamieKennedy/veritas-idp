using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Veritas.Admin.API.Extensions;
using Veritas.MessagingService.Domain.Types;
using Veritas.UserService.Domain.Entities;
using Xunit;

namespace Veritas.Admin.API.Tests.Configuration;

public sealed class AdminControllerExtensionsTests
{
    [Fact]
    public void AddAdminControllers_uses_string_enum_wire_values()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAdminControllers();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

        Assert.Equal("\"MfaEnrollment\"", JsonSerializer.Serialize(EAdminLoginChallengePurpose.MfaEnrollment, options));
        Assert.Equal(SmtpTlsMode.StartTls, JsonSerializer.Deserialize<SmtpTlsMode>("\"StartTls\"", options));
    }
}
