# Admin API Agent Guide

Scope: `src/backend/edge/admin-api`.

The Admin API is the admin-facing HTTP API host for Veritas. It should stay thin: expose versioned HTTP endpoints, validate request shape, compose backend modules in-process, call module Application services, and map module failures into appropriate HTTP responses. Business rules belong in the owning backend module.

## Current Shape

- Project: `Veritas.Admin.API`
- Startup: `Veritas.Admin.API/Program.cs`
- Controllers: `Veritas.Admin.API/Controllers/v1`
- HTTP models: `Veritas.Admin.API/Models`
- Module composition and OpenAPI/versioning setup: `Veritas.Admin.API/Extensions/ServiceExtensions.cs`
- Wolverine/RabbitMQ setup: `Veritas.Admin.API/Extensions/MessagingExtensions.cs`
- Async message handlers currently live under `Veritas.Admin.API/Modules`

Current behavior:

- `BootstrapController` exposes bootstrap status and completion endpoints.
- Bootstrap requests require `SETUP_TOKEN`, injected by Aspire as an environment variable.
- Admin API composes Platform and Users modules in-process. Bootstrap calls Platform Application services directly.
- `ServiceExtensions.AddVeritasModules` registers module DbContexts, module Application services, and in-process cross-module adapters.
- `AdminUserController` currently contains placeholder create behavior and should not become the owner of admin-user business logic.
- Wolverine is configured for RabbitMQ-backed async messages such as `SendEmailRequestedV1`.
- Scalar/OpenAPI is enabled in development.

## Rules

- Do not put domain or persistence logic in controllers.
- Keep HTTP translation in controllers, not in Domain/Application projects.
- Do not add internal gRPC clients for module-to-module calls. Use in-process module Application services and consumer-owned ports.
- Use API versioning conventions already configured: URL segment, `api-version` query parameter, and `X-Api-Version` header.
- Keep route shape consistent with `api/v{version:apiVersion}/[controller]`.
- Map `FluentResults` and module failures intentionally. Avoid returning internal exception details directly to callers.
- Never log setup tokens, passwords, bootstrap secrets, OTPs, or raw session/client secrets.
- Keep request/response models specific to the Admin API. Do not reuse EF entities as HTTP contracts.
- Keep module composition in extension methods or small adapter classes; controllers should not manually assemble cross-module dependencies.
- Async side effects should be published through Wolverine/RabbitMQ rather than performed inline in request handlers.

## Dependencies

- References Platform/User Application and Infrastructure projects to compose modules in-process.
- References `Veritas.Contracts.Messages` for durable async message contracts.
- References `Veritas.ServiceDefaults` for logging, telemetry, discovery, resilience, and health checks.
- Does not use service discovery for internal Platform/User calls.

## Verification

From the repo root:

```powershell
dotnet build Veritas.slnx
dotnet run --project src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj
```

Use the Aspire dashboard or Admin API Scalar page in development to inspect endpoints.
