# Aspire Orchestration Agent Guide

Scope: `src/orchestration/aspire`.

Follow the repository-wide coding standards in [`../../../docs/engineering/coding-standards.md`](../../../docs/engineering/coding-standards.md) in addition to the orchestration-specific rules below.

The Aspire AppHost is the local development orchestrator for Veritas. It is not the production architecture by itself; keep changes compatible with the intended Docker Compose self-hosting story.

## Current Shape

- AppHost project: `Veritas.AppHost`
- Main file: `Veritas.AppHost/AppHost.cs`
- Local resources:
    - Redis
    - Postgres on port `5432`, with persisted volume `veritas-postgres-data`
    - RabbitMQ with management plugin on port `15672`, with persisted volume `veritas-rabbitmq-data`
    - `Veritas.Tooling.DbMigrator`
    - Admin API host (`Veritas.Admin.API`)

## Rules

- Keep resource names stable, especially `VeritasDb` and `messaging`, because API hosts and Wolverine use those connection/resource names.
- Keep the migrator waiting for Postgres and API hosts waiting for the migrator when persistence is required.
- Keep this ordering aligned with Docker Compose. Release packaging uses one shared backend image, but the migrator remains a distinct one-shot process and service.
- Add new persisted infrastructure here only when a module or API host actually needs it locally.
- Add secret values as Aspire parameters, not literals in `AppHost.cs`.
- The `bootstrap-secret` parameter is secret and is injected into Admin API as `BOOTSTRAP_SECRET`.
- Do not re-add Platform/User service API resources for normal modular-monolith development. Internal module calls should be in-process through the API host.
- When adding a new API host, wire service defaults, required infrastructure references, explicit wait relationships, and useful dashboard endpoint links.
- Keep RabbitMQ referenced by API hosts that configure Wolverine with `UseRabbitMqUsingNamedConnection("messaging")`.

## Verification

From the repo root:

```powershell
pnpm dev
```

Use the direct AppHost command when debugging Aspire startup or when pnpm is unavailable:

```powershell
dotnet run --project src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj
```

If AppHost fails because `bootstrap-secret` is missing, configure it via user secrets or environment variable as documented in `docs/bootstrap.md`.
