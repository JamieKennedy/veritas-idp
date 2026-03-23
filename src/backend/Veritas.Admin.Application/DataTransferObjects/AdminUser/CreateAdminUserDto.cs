namespace Veritas.Admin.Application.DataTransferObjects.AdminUser;

public class CreateAdminUserDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}