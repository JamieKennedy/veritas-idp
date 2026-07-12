#pragma warning disable ASPIRECERTIFICATES001

using Aspire.Hosting.JavaScript;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

const int adminApiHttpPort = 5100;
const int adminApiHttpsPort = 7100;
const int adminUiHttpsPort = 3000;

var redis = builder.AddRedis("redis");

var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("veritas-postgres-data");

var veritasDb = postgres.AddDatabase("VeritasDb");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume("veritas-rabbitmq-data")
    .WithManagementPlugin(port: 15672);

var migrator = builder.AddProject<Veritas_Tooling_DbMigrator>("migrator")
    .WithReference(veritasDb)
    .WaitFor(veritasDb);

// Deployment provides this secret (user-secrets locally, secret store/env in deployed environments).
var bootstrapSecret = builder.AddParameter("bootstrap-secret", secret: true);
var dataProtectionKeysPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Veritas",
    "DataProtectionKeys");

var adminApi = builder.AddProject<Veritas_Admin_API>("veritas-admin-api")
    .WithHttpEndpoint(port: adminApiHttpPort)
    .WithHttpsEndpoint(port: adminApiHttpsPort)
    .WithReference(veritasDb)
    .WithReference(rabbitmq)
    .WaitFor(veritasDb)
    .WaitFor(rabbitmq)
    .WaitFor(migrator)
    .WithEnvironment("BOOTSTRAP_SECRET", bootstrapSecret)
    .WithEnvironment("DataProtection__KeysPath", dataProtectionKeysPath)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar";
        url.Url = "/scalar";
    });

var adminUI = builder.AddViteApp("veritas-admin-ui", "../../../frontend/admin-ui")
    .WithPnpm()
    .WithHttpsEndpoint(port: adminUiHttpsPort, env: "PORT")
    .WithHttpsDeveloperCertificate()
    .WithEnvironment("VITE_ADMIN_API_BASE_URL", adminApi.GetEndpoint("https"))
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Admin UI";
    });

adminApi.WithEnvironment("Cors__AllowedOrigins__0", adminUI.GetEndpoint("https"));

builder.Build().Run();
