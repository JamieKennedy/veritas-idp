using Veritas.Admin.Application.DataTransferObjects.AdminUser;

namespace Veritas.Admin.Application.DataTransferObjects.Setup;

public class SetupDto
{
    public required CreateAdminUserDto AdminUser { get; set; }
    public required string SetupToken { get; set; }
}