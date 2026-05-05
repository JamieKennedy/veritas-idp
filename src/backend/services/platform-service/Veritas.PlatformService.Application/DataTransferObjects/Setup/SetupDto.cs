using Veritas.PlatformService.Application.DataTransferObjects.AdminUser;

namespace Veritas.PlatformService.Application.DataTransferObjects.Setup;

public class SetupDto
{
    public required CreateAdminUserDto AdminUser { get; set; }
    public required string SetupToken { get; set; }
}
