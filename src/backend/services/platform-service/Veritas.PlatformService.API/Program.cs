using Veritas.PlatformService.API.Services;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Services;
using Veritas.PlatformService.Domain.Repositories;
using Veritas.PlatformService.Infrastructure.Database;
using Veritas.PlatformService.Infrastructure.Database.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSeqEndpoint("seq");
builder.AddNpgsqlDbContext<PlatformDbContext>(
    "VeritasDb",
    configureDbContextOptions: optionsBuilder => PlatformDbContextOptions.Configure(optionsBuilder));

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<ISystemFlagRepository, SystemFlagRepository>();
builder.Services.AddScoped<ISystemFlagService, SystemFlagService>();

var app = builder.Build();

app.MapGrpcService<PlatformSetupGrpcService>();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
