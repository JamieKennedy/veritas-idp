namespace Veritas.Admin.API.Models.AdminUser;

public class CreateAdminUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
