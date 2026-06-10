# Messaging Service Agent Guide

Scope: `src/backend/services/messaging-service`.

The Messaging module owns outbound notification delivery concerns for Veritas: SMTP settings, email adapters, template storage, placeholder validation, rendering, tenant-ready template overrides, and delivery orchestration.

## Ownership

Messaging owns:

- SMTP configuration and encrypted SMTP secrets.
- Email template storage, validation, rendering, and enabled state.
- Email delivery adapters and delivery failure handling.
- Tenant-ready template lookup using nullable `TenantId` without cross-module foreign keys.

Messaging does not own:

- Users, credentials, bootstrap sessions, OAuth clients, sessions, signing keys, audit storage, or webhooks.
- HTTP authentication or authorization policy. Admin API exposes authenticated management endpoints and calls Messaging Application services.

## Layer Rules

- Domain contains entities, enums, and stable Messaging errors.
- Application contains use cases, DTOs, template rendering, delivery orchestration, and `IMessagingDbContext`.
- Infrastructure contains EF Core DbContext, migrations, SMTP adapter implementations, and persistence registration.
- Do not log SMTP secrets, protected secret payloads, rendered email bodies, OTPs, setup tokens, passwords, session tokens, or client secrets.
- Durable message contracts must stay under `src/backend/building-blocks/contracts/Veritas.Contracts.Messages` and must not include raw secrets or rendered sensitive content.

## Verification

From the repo root:

```powershell
dotnet test src\backend\services\messaging-service\Veritas.MessagingService.Application.Tests\Veritas.MessagingService.Application.Tests.csproj
dotnet build Veritas.slnx
dotnet test Veritas.slnx
```
