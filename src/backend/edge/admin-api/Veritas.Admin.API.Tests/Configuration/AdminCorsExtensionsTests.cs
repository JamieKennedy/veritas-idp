using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Veritas.Admin.API.Extensions;

using Xunit;

namespace Veritas.Admin.API.Tests.Configuration;

public sealed class AdminCorsExtensionsTests
{
    [Fact]
    public void AddAdminUiCorsPolicy_allows_configured_admin_ui_origin_with_credentials()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddAdminUiCorsPolicy(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>>();
        var policy = options.Value.GetPolicy(AdminCorsExtensions.AdminUiPolicyName);

        Assert.NotNull(policy);
        Assert.Contains("http://localhost:3000", policy.Origins);
        Assert.True(policy.SupportsCredentials);
        Assert.True(policy.AllowAnyHeader);
        Assert.True(policy.AllowAnyMethod);
    }
}
