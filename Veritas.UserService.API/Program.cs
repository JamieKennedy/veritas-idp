using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Veritas.UserService.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<UserDbContext>(
    "VeritasDb",
    configureDbContextOptions: optionsBuilder => UserDbContextOptions.Configure(optionsBuilder));

builder.AddSeqEndpoint("seq");
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

