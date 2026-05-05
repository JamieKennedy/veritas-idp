using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Veritas.PlatformService.Infrastructure.Database;
using Veritas.Tooling.DbMigrator;
using Veritas.UserService.Infrastructure.Database;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PlatformDbContext>(
    "VeritasDb",
    configureDbContextOptions: optionsBuilder => PlatformDbContextOptions.Configure(optionsBuilder));
builder.AddNpgsqlDbContext<UserDbContext>(
    "VeritasDb",
    configureDbContextOptions: optionsBuilder => UserDbContextOptions.Configure(optionsBuilder));

var host = builder.Build();

await Utilities.MigrateAsync<PlatformDbContext>(host.Services);
await Utilities.MigrateAsync<UserDbContext>(host.Services);
return;



