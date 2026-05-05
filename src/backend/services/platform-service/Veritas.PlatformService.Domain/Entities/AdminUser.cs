namespace Veritas.PlatformService.Domain.Entities;

public class AdminUser
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; } 
    public string? Name { get; set; }
}
