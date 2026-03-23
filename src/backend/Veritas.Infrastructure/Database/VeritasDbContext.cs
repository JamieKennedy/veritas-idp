using Microsoft.EntityFrameworkCore;
using Veritas.Domain.Entities;

namespace Veritas.Infrastructure.Database;

public class VeritasDbContext : DbContext
{    
    public VeritasDbContext(DbContextOptions<VeritasDbContext> options) : base(options)
    {
    }
    
    public DbSet<SystemFlag> SystemFlags { get; set; }
}