using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Contracts.Grpc;

namespace Veritas.PlatformService.API.Services;

public sealed class PlatformSetupGrpcService(
    ISystemFlagService systemFlagService,
    ILogger<PlatformSetupGrpcService> logger) : PlatformSetup.PlatformSetupBase
{
    private const string SetupCompletedFlagKey = "SETUP_COMPLETED";

    public override async Task<GetSetupStatusResponse> GetSetupStatus(Empty request, ServerCallContext context)
    {
        var flagResult = await systemFlagService.GetFlagValue(SetupCompletedFlagKey);

        return new GetSetupStatusResponse
        {
            IsSetupCompleted = flagResult.IsSuccess && flagResult.Value
        };
    }

    public override async Task<CompleteSetupResponse> CompleteSetup(CompleteSetupRequest request, ServerCallContext context)
    {
        var adminUser = request.AdminUser;

        if (adminUser is null || string.IsNullOrWhiteSpace(adminUser.Email) || string.IsNullOrWhiteSpace(adminUser.Password))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Admin user email and password are required."));
        }

        // Admin user orchestration will move behind its own service boundary once the User Service exists.
        var result = await systemFlagService.SetFlagValue(SetupCompletedFlagKey, true);

        if (result.IsFailed)
        {
            logger.LogError("Failed to mark platform setup as completed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
            throw new RpcException(new Status(StatusCode.Internal, "Failed to complete platform setup."));
        }

        logger.LogInformation("Platform setup marked as completed.");

        return new CompleteSetupResponse
        {
            IsSetupCompleted = true
        };
    }
}
