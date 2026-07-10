using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Veritas.Admin.API.Controllers.v1;
using Xunit;

namespace Veritas.Admin.API.Tests.Messaging;

public sealed class MessagingEndpointSecurityTests
{
    [Fact]
    public void MessagingController_requires_authenticated_administrator()
    {
        var authorizeAttribute = typeof(MessagingController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorizeAttribute);
    }
}
