using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.Admin.API.Extensions;

using Xunit;

namespace Veritas.Admin.API.Tests.Http;

public sealed class AdminApiExceptionHandlerTests
{
    [Fact]
    public async Task Returns_a_safe_problem_with_a_trace_identifier()
    {
        await using var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        await using var responseBody = new MemoryStream();
        var context = new DefaultHttpContext
        {
            RequestServices = services,
            TraceIdentifier = "test-request"
        };
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.Body = responseBody;
        var handler = new AdminApiExceptionHandler(
            NullLogger<AdminApiExceptionHandler>.Instance,
            services.GetRequiredService<IProblemDetailsService>());

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("internal provider detail"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        responseBody.Position = 0;
        using var response = await JsonDocument.ParseAsync(responseBody);
        Assert.Equal(500, response.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("test-request", response.RootElement.GetProperty("traceId").GetString());
        Assert.DoesNotContain("internal provider detail", response.RootElement.GetRawText(), StringComparison.Ordinal);
    }
}
