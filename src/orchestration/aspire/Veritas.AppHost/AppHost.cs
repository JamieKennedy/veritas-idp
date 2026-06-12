using Projects;

var builder = DistributedApplication.CreateBuilder(args);

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

builder.AddProject<Veritas_Admin_API>("veritas-admin-api")
    .WithHttpEndpoint(targetPort: 5100, port: 5100)
    .WithHttpsEndpoint(targetPort: 7100, port: 7100)
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

builder.Build().Run();
