using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", port: 5341)
    .WithDataVolume("veritas-seq-data");

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

builder.AddProject<Veritas_Admin_API>("veritas-admin-api")
    .WithReference(veritasDb)
    .WithReference(seq)
    .WithReference(rabbitmq)
    .WaitFor(veritasDb)
    .WaitFor(seq)
    .WaitFor(rabbitmq)
    .WaitFor(migrator)
    .WithEnvironment("BOOTSTRAP_SECRET", bootstrapSecret)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar";
        url.Url = "/scalar";
    });

builder.Build().Run();
