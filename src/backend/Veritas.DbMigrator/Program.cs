using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Veritas.Infrastructure.Database;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<VeritasDbContext>("VeritasDb");

var host = builder.Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<VeritasDbContext>();

await db.Database.EnsureCreatedAsync();
await db.Database.MigrateAsync();
