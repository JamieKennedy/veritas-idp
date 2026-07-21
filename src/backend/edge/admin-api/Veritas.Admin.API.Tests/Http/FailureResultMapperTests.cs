using FluentResults;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Veritas.Shared.Errors;
using Veritas.Shared.Http;

using Xunit;

namespace Veritas.Admin.API.Tests.Http;

public sealed class FailureResultMapperTests
{
    [Fact]
    public void ToProblemDetails_maps_validation_metadata_to_bad_request()
    {
        var result = Result.Fail(new Error("The administrator password must be at least 12 characters long.")
            .WithMetadata(ErrorMetadataKeys.Category, ErrorCategories.Validation)
            .WithMetadata(ErrorMetadataKeys.Code, "USERS_ADMIN_PASSWORD_TOO_SHORT"));

        var actionResult = FailureResultMapper.ToProblemDetails(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Validation failed.", problemDetails.Title);
        Assert.Equal("The administrator password must be at least 12 characters long.", problemDetails.Detail);
        Assert.Equal("USERS_ADMIN_PASSWORD_TOO_SHORT", problemDetails.Extensions["errorCode"]);
        Assert.Equal("Validation", problemDetails.Extensions["errorCategory"]);
    }

    [Fact]
    public void ToProblemDetails_maps_conflict_metadata_to_conflict()
    {
        var result = Result.Fail(new Error("The first administrator account has already been created.")
            .WithMetadata(ErrorMetadataKeys.Category, ErrorCategories.Conflict)
            .WithMetadata(ErrorMetadataKeys.Code, "USERS_INITIAL_ADMIN_ALREADY_EXISTS"));

        var actionResult = FailureResultMapper.ToProblemDetails(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("Conflict.", problemDetails.Title);
        Assert.Equal("The first administrator account has already been created.", problemDetails.Detail);
        Assert.Equal("USERS_INITIAL_ADMIN_ALREADY_EXISTS", problemDetails.Extensions["errorCode"]);
        Assert.Equal("Conflict", problemDetails.Extensions["errorCategory"]);
    }

    [Fact]
    public void ToProblemDetails_honors_explicit_http_status_metadata()
    {
        var result = Result.Fail(new Error("The bootstrap session has expired.")
            .WithMetadata(ErrorMetadataKeys.Category, ErrorCategories.Conflict)
            .WithMetadata(ErrorMetadataKeys.Code, "PLATFORM_BOOTSTRAP_SESSION_EXPIRED")
            .WithMetadata(ErrorMetadataKeys.HttpStatusCode, "410"));

        var actionResult = FailureResultMapper.ToProblemDetails(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status410Gone, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status410Gone, problemDetails.Status);
        Assert.Equal("Gone.", problemDetails.Title);
        Assert.Equal("The bootstrap session has expired.", problemDetails.Detail);
        Assert.Equal("PLATFORM_BOOTSTRAP_SESSION_EXPIRED", problemDetails.Extensions["errorCode"]);
    }

    [Fact]
    public void ToProblemDetails_maps_unclassified_errors_to_generic_internal_server_error()
    {
        var result = Result.Fail("Provider failed with internal details.");

        var actionResult = FailureResultMapper.ToProblemDetails(result);

        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("Unexpected failure.", problemDetails.Title);
        Assert.Equal("The request failed unexpectedly.", problemDetails.Detail);
    }
}
