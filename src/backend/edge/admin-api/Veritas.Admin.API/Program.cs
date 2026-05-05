using Scalar.AspNetCore;
using Veritas.Admin.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.AddSeqEndpoint("seq");
builder.Services.ConfigurePlatformGrpcClients();

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
