using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", port: 5341)
    .WithDataVolume("veritas-seq-data");

var redis = builder.AddRedis("redis");



var postgres = builder.AddPostgres("postgres", port: 5432)
    .WithDataVolume("veritas-postgres-data");

var veritasDb = postgres.AddDatabase("VeritasDb");

var migrator = builder.AddProject<Veritas_Tooling_DbMigrator>("migrator")
    .WithReference(veritasDb)
    .WaitFor(veritasDb);

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume("veritas-rabbitmq-data")
    .WithManagementPlugin(port: 15672);

// Deployment provides this secret (user-secrets locally, secret store/env in deployed environments).
var setupToken = builder.AddParameter("setup-token", secret: true);

var platformServiceApi = builder.AddProject<Veritas_PlatformService_API>("platform-service-api")
    .WithReference(veritasDb)
    .WithReference(seq)
    .WaitFor(veritasDb)
    .WaitFor(seq)
    .WaitFor(migrator);

var adminAPI = builder.AddProject<Veritas_Admin_API>("veritas-admin-api")
    .WithReference(seq)
    .WithReference(platformServiceApi)
    .WaitFor(seq)
    .WaitFor(platformServiceApi)
    .WithEnvironment("SETUP_TOKEN", setupToken)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar";
        url.Url = "/scalar";
    });

builder.Build().Run();

