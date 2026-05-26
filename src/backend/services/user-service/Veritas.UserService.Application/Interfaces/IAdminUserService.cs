using FluentResults;

namespace Veritas.UserService.Application.Interfaces;

public interface IAdminUserService
{
    Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default);
}
