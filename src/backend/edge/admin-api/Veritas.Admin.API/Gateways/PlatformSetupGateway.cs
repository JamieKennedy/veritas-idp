using Google.Protobuf.WellKnownTypes;
using Veritas.Admin.API.Models.Setup;
using Veritas.PlatformService.Contracts.Grpc;

namespace Veritas.Admin.API.Gateways;

public sealed class PlatformSetupGateway(PlatformSetup.PlatformSetupClient client)
{
    public async Task<bool> GetSetupStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.GetSetupStatusAsync(new Empty(), cancellationToken: cancellationToken);
        return response.IsSetupCompleted;
    }

    public async Task CompleteSetupAsync(SetupRequest request, CancellationToken cancellationToken = default)
    {
        var grpcRequest = new CompleteSetupRequest
        {
            AdminUser = new AdminUser
            {
                Email = request.AdminUser.Email,
                Password = request.AdminUser.Password
            }
        };

        await client.CompleteSetupAsync(grpcRequest, cancellationToken: cancellationToken);
    }
}
