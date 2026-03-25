using Scalar.AspNetCore;
using Veritas.Admin.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuredSetupToken = builder.Configuration["SETUP_TOKEN"];
if (string.IsNullOrWhiteSpace(configuredSetupToken))
{
    throw new InvalidOperationException("SETUP_TOKEN is missing. Configure it before starting Veritas.Admin.API.");
}


builder.AddServiceDefaults();

// Add services to the container.
builder.AddSeqEndpoint("seq");
builder.ConfigureSqlContext();
builder.Services.ConfigureRepositories();
builder.Services.ConfigureServices();


builder.Services.ConfigureVersioning();

builder.Services.AddControllers();

builder.Services.ConfigureOpenApi();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Configure Scalar
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Veritas Admin API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();