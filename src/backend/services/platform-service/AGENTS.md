# Platform Service Agent Guide

Scope: `src/backend/services/platform-service`.

Follow the repository-wide coding standards in [`../../../../docs/engineering/coding-standards.md`](../../../../docs/engineering/coding-standards.md) in addition to the module-specific rules below.

The Platform module owns global platform and instance-level state for Veritas. It is the place for bootstrap/setup state, system flags, feature flags, instance metadata, and other configuration that is not owned by a tenant, user, client, session, or key module.

## Current Shape

- Application project: `Veritas.PlatformService.Application`
- Application tests project: `Veritas.PlatformService.Application.Tests`
- Domain project: `Veritas.PlatformService.Domain`
- Infrastructure project: `Veritas.PlatformService.Infrastructure`
- DbContext: `PlatformDbContext`
- Migration history table: `__EFMigrationsHistory_Platform`
- Module-owned Application DbContext port: `IPlatformDbContext`
- Consumer-owned Users dependency port: `IAdminUserDirectory`

Current domain/application concepts:

- `SystemFlag` stores boolean global flags.
- `BootstrapSession` models bootstrap session state.
- `BootstrapSessionStatus` defines bootstrap session lifecycle states.
- `ISystemFlagService` / `SystemFlagService` manage system flags.
- `IBootstrapService` / `BootstrapService` orchestrate no-email bootstrap status/start/complete logic and durable SMTP setup deferral.
- Platform Application calls `IAdminUserDirectory` to inspect admin-user state during bootstrap. API hosts provide an in-process adapter to Users Application.
- Platform Application calls `IInitialAdminCreator` to create the first admin through Users Application during bootstrap completion.

## Ownership

Platform module owns:

- Setup/bootstrap status and state machine.
- System-wide flags and instance metadata.
- Future feature flags and global settings.
- Platform-level readiness decisions.

Platform module does not own:

- User records, credentials, password hashes, MFA factors, or recovery codes.
- OAuth clients, redirect URIs, secrets, grant types, or scopes.
- Sessions, refresh tokens, authorization codes, signing keys, audit logs, notifications, or webhooks.

## Layer Rules

- Keep platform rules and entities in Domain.
- Keep bootstrap orchestration, `FluentResults` failure handling, `IPlatformDbContext`, and consumer-owned dependency ports in Application.
- Keep EF Core DbContexts, migrations, Redis, and external adapters in Infrastructure.
- Avoid repositories by default. Use EF Core directly in Application through `IPlatformDbContext`; Application services decide when to call `SaveChangesAsync`.
- Do not add gRPC clients into Domain or Application. Cross-module calls belong behind Application ports such as `IAdminUserDirectory`.
- Keep bootstrap errors typed where useful, for example `SystemAlreadyConfiguredError`.

## Bootstrap Guidance

Bootstrap is security-sensitive.

- Store bootstrap secrets and session tokens only as hashes.
- Use UTC expiry and completion timestamps.
- Keep the state machine explicit: verified, completed, expired, cancelled. `PendingVerification` may exist historically but first-admin bootstrap should not depend on email verification.
- Do not leak whether a particular email exists or which part of bootstrap failed through public-facing errors.
- Once User Service creates the initial admin user, Platform Service should mark bootstrap complete by durable platform state, not only by transient cache.
- Do not add SMTP/Mailgun checks or OTP sending to bootstrap. Email delivery is configured after first-admin setup.

## Data Guidance

- Add Platform module migrations in `Veritas.PlatformService.Infrastructure`.
- Keep service-specific migration history through `PlatformDbContextOptions`.
- Platform tables should use the module-owned `platform` schema where possible.
- Do not add user-owned tables here. Reference external service identifiers only when there is a true platform-level setting attached to them.
- Do not inject or query `UserDbContext`. Ask for Users state through `IAdminUserDirectory` or async messages.
- Redis can cache system flags/settings later, but Postgres remains the source of truth.

## Verification

From the repo root:

```powershell
dotnet build Veritas.slnx
dotnet run --project src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj
```

When changing Platform Application behavior, add/update focused tests in `Veritas.PlatformService.Application.Tests` and rebuild the solution.
