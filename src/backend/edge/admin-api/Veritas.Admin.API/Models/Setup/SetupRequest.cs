using Veritas.Admin.API.Models.AdminUser;

namespace Veritas.Admin.API.Models.Setup;

public class SetupRequest
{
    public required CreateAdminUserRequest AdminUser { get; set; }
    public required string SetupToken { get; set; }
}
