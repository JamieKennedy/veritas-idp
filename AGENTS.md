# Veritas Agent Guide

Veritas is a self-hostable auth service / identity provider. The backend is a .NET 10 modular monolith with separate HTTP API hosts for different audiences, local development is orchestrated with .NET Aspire, and the intended deployment shape is Docker Compose. Backend business capability is organized into modules that follow onion architecture: Domain at the center, Application around use cases, Infrastructure for persistence and external integrations, and API hosts at the edge.

This file applies to the whole repository. More specific `AGENTS.md` files exist under module, API-host, and orchestration directories and should be read when working in those scopes.

## Engineering Priorities

Veritas is security-sensitive infrastructure. Robustness, observability, and defensive defaults are core project requirements, not polish.

- Prefer designs that fail clearly, recover predictably, and leave the system in a known state. Startup tasks, migrations, background processing, messaging, and bootstrap/setup flows should handle retries, cancellation, idempotency, and partial failure deliberately.
- Treat logging as operational evidence. Important state transitions, security-sensitive actions, startup work, migrations, retries, and failures should emit structured logs with enough context to diagnose the issue without exposing secrets.
- Security must shape the implementation from the start. Validate inputs, minimize trust boundaries, avoid secret exposure, and choose conservative defaults even when the convenient path is faster.
- When changing critical paths, consider how the feature behaves during restarts, duplicate requests, concurrent execution, unavailable dependencies, and malformed or hostile input.
- Make failure modes explicit in code and tests where practical. Avoid swallowing exceptions unless the caller gets a clear result and the logs preserve useful diagnostic context.

## Current Solution Map

- `Veritas.slnx` is the solution entry point.
- `src/backend/building-blocks/contracts/Veritas.Contracts.Messages` contains durable async message contracts used with Wolverine/RabbitMQ. Keep messages versioned, stable, and free of raw secrets.
- `src/backend/building-blocks/service-defaults/Veritas.ServiceDefaults` contains shared Aspire defaults: service discovery, HTTP resilience, health checks, OpenTelemetry, and Serilog file logging.
- `src/backend/building-blocks/shared/Veritas.Shared` is reserved for shared backend code. Keep it small and avoid turning it into a domain dumping ground.
- `src/backend/edge/admin-api` is the admin-facing HTTP API host. It composes backend modules in-process and exposes Scalar/OpenAPI in development.
- `src/backend/edge/public-api` is reserved for the future public/end-user HTTP API host. It should compose the same internal modules in-process without sharing admin-only controllers or HTTP models.
- `src/backend/services/platform-service` is the Platform module. It owns global platform state, bootstrap/setup state, system flags, and instance-level configuration.
- `src/backend/services/user-service` is the Users module. It owns user identity data and admin-user state.
- `src/backend/tooling/db-migrator` runs EF Core migrations for module DbContexts during Aspire startup.
- `src/frontend/admin-ui` and `src/frontend/public-ui` are reserved pnpm workspace packages for future React apps.
- `src/orchestration/aspire/Veritas.AppHost` defines local dev orchestration for Seq, Redis, Postgres, RabbitMQ, the migrator, and Admin API.

## Architecture Rules

- Preserve module boundaries. A module owns its data and write model; other modules must not reach into that module's DbContext or tables directly.
- Use synchronous in-process application interfaces for immediate user-facing validation and orchestration. Do not add new internal gRPC calls between modules unless a module is intentionally extracted into a separate process.
- Cross-module synchronous interfaces are consumer-owned ports in the consuming module's Application layer. API hosts or infrastructure adapters implement those ports by calling the provider module's Application services.
- Use Wolverine/RabbitMQ asynchronous messages for side effects such as audit, notifications, webhooks, projections, cleanup, retries, and cross-module reactions that should not block the caller.
- Keep Redis as cache/ephemeral state only. It should not become the source of truth for users, clients, sessions, setup completion, or keys.
- Use one Postgres database with module-owned schemas/tables and separate module DbContexts/migrations. Keep DbContext ownership isolated.
- Do not add business logic to API hosts. API hosts validate HTTP request shape, call module Application services, and map failures to HTTP responses.
- Keep durable message contracts versioned and backward-compatible.

## Onion Architecture

Use this dependency direction inside each module:

- `Domain`: entities, value objects, enums, domain errors, and core business rules. No EF Core, ASP.NET Core, gRPC, or infrastructure concerns.
- `Application`: use-case services, commands/queries, DTOs, orchestration inside the module boundary, result mapping, module-owned DbContext interfaces, and consumer-owned ports needed by the use case.
- `Infrastructure`: EF Core DbContexts, migrations, external providers, cache/messaging adapters, and implementation details.
- API hosts: ASP.NET Core startup, controllers/endpoints, request/response mapping, middleware, health checks, versioning, and module composition.

Allowed project references should flow inward plus composition at the API:

- `Domain` references no service layer.
- `Application` references `Domain` and may reference EF Core abstractions for module-owned DbContext interfaces.
- `Infrastructure` references `Application` and `Domain` so DbContexts can implement Application persistence interfaces.
- API hosts reference module `Application` and `Infrastructure` projects, message contracts, and service defaults.

## Backend Style

- Target .NET 10, nullable reference types enabled, implicit usings enabled.
- Follow existing namespaces and folder naming: `Veritas.<Service>.<Layer>`.
- Prefer constructor injection. Primary constructors are already used in several services; use them when they keep the class readable.
- Use `FluentResults` in Application services for recoverable domain/application failures. Map those failures to HTTP status codes at API-host boundaries.
- Keep controllers thin. They should validate transport-level inputs, call Application services, log failures, and return HTTP responses.
- Use EF Core directly in Application through module-owned DbContext interfaces such as `IPlatformDbContext` and `IUserDbContext`.
- Avoid repositories by default. EF Core already provides repository/unit-of-work behavior; add a focused persistence service only for complex persistence operations that deserve a name.
- Application use cases own transaction timing and call `SaveChangesAsync` once per atomic operation. Do not call `SaveChangesAsync` from lower-level helpers.
- Use UTC timestamps with `Utc` suffix in persisted models.
- Keep secrets out of logs and source control. This is especially important for setup tokens, bootstrap secrets, passwords, OTPs, session tokens, client secrets, signing keys, and recovery codes.
- Never expose password hashes or raw secrets in API responses, transition transport responses, messages, or logs.
- Prefer structured logs with named properties over string interpolation.
- Log failures at the boundary that can add the most useful context, then return controlled errors. Avoid duplicate noisy logs for the same failure unless each log adds distinct operational information.
- For dependency calls such as Postgres, Redis, RabbitMQ, email, and future external identity providers, handle unavailable or slow dependencies with timeouts, cancellation, retry/backoff where appropriate, and clear error reporting.

## Code Style

- All methods must have XML documentation comments (constructors can be ommited unless there is a reason).
- Method XML docs should explain what the method does, describe parameters, document return values, and list known exceptions with `<exception>` tags.
- Public properties and non-obvious internal properties should have XML documentation that explains what the value represents, including important type/format expectations such as UTC timestamps, hashed secrets, identifiers, and nullable meanings.
- Keep XML docs accurate when changing behavior. Do not leave stale summaries, incorrect return descriptions, or missing exception notes after refactors.
- Prefer concise documentation that captures contract and intent. Avoid restating the method name without adding useful meaning.

## Contracts

- Put durable async message contracts under `src/backend/building-blocks/contracts/Veritas.Contracts.Messages`.
- Version message contract type names, for example `SendEmailRequestedV1`.
- Message contracts must include safe payloads only. Never include raw passwords, setup tokens, OTPs, session tokens, client secrets, signing keys, or recovery codes.

## Data And Migrations

- Platform module uses `PlatformDbContext` and migration history table `__EFMigrationsHistory_Platform`.
- Users module uses `UserDbContext` and migration history table `__EFMigrationsHistory_User`.
- The Aspire AppHost starts `Veritas.Tooling.DbMigrator` before API hosts. Add new DbContexts there when new modules gain persistence.
- Migrations live inside each module's Infrastructure project.
- Prefer schema-per-module, for example `platform.*`, `users.*`, `messaging.*`, and `audit.*`.
- Avoid cross-module foreign keys. Store identifiers for integration only when needed.
- Do not inject or query another module's DbContext. If a use case needs cross-module data, use a consumer-owned Application port or async message.

## Local Development

Useful commands from the repository root:

```powershell
dotnet build Veritas.slnx
dotnet run --project src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj
```

The AppHost requires the secret parameter `setup-token`, which is injected into Admin API as `SETUP_TOKEN`. See `docs/setup-token.md`.

Current Aspire resources:

- Seq on port `5341`
- Postgres on port `5432`
- RabbitMQ management on port `15672`
- Admin API with Scalar linked from the Aspire dashboard

## Testing And Verification

- Focused backend test projects live beside the module project they test, for example `Veritas.PlatformService.Application.Tests` beside `Veritas.PlatformService.Application`. When adding meaningful behavior, prefer Application-level tests around module use cases and ports.
- At minimum, run `dotnet build Veritas.slnx` after backend edits when dependencies are available.
- Run `dotnet test Veritas.slnx` after behavior changes when dependencies are available.
- For message contract changes, build the solution and exercise the publishing/handler path if possible.
- For EF model changes, add a migration in the owning service Infrastructure project and confirm the migrator still builds.
- For startup, migration, messaging, and security-sensitive changes, verify at least one failure path as well as the happy path when feasible.

## Security Posture

Veritas is an IDP, so correctness and defensive defaults matter more than convenience.

- Validate and normalize identity inputs deliberately, especially emails, usernames, redirect URIs, scopes, client ids, and callback URLs.
- Hash all credentials and one-time secrets. Store token/session/OTP hashes, not raw values.
- Compare secrets using constant-time helpers when available.
- Keep bootstrap/setup paths narrow, auditable, and easy to disable after setup.
- Prefer least privilege and narrow data access. A module should only access the data, secrets, and dependencies it needs to complete its own responsibility.
- Do not leak account existence, credential validity, token state, setup state internals, or service topology through overly specific external errors.
- Sanitize logs, traces, metrics, exceptions, and validation responses before they can expose credentials, tokens, hashes, internal topology, or user-sensitive data.
- Treat concurrency and replay as security concerns. Bootstrap, setup, credential, token, and admin-user flows should be idempotent or explicitly guarded against duplicate execution.
- Add audit/event hooks for security-sensitive changes as the audit/event infrastructure lands.

## Future Service Direction

The planning notes describe additional modules and API hosts that are not fully implemented yet: Auth, Client/Project, Session, Key Management, Messaging/Notification, Audit, Webhooks, Public API, Admin UI, and Public UI. When adding backend capability modules, follow the same module-root structure and add a scoped `AGENTS.md` in the new module directory. When adding API hosts, keep HTTP concerns in the host and compose modules in-process.

## Working With This Repo

- The worktree may contain active user edits. Do not revert or move unrelated changes.
- Ignore build output under `bin`, `obj`, logs, and IDE metadata unless the task explicitly targets them.
- Prefer small, module-scoped changes over broad refactors.
- If a change crosses a module boundary, update ports/message contracts and the relevant scoped `AGENTS.md` files.
