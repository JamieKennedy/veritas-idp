# User Service Agent Guide

Scope: `src/backend/services/user-service`.

The Users module owns identities and credential-related user data. In the current implementation it is small, but it is the future home for public users, admin users, emails/usernames/phones, password hashes, MFA enrollment, recovery codes, verification state, account status, and user lifecycle events.

## Current Shape

- Application project: `Veritas.UserService.Application`
- Application tests project: `Veritas.UserService.Application.Tests`
- Domain project: `Veritas.UserService.Domain`
- Infrastructure project: `Veritas.UserService.Infrastructure`
- DbContext: `UserDbContext`
- Migration history table: `__EFMigrationsHistory_User`
- Module-owned Application DbContext port: `IUserDbContext`

Current domain/application concepts:

- `AdminUser` stores admin identity fields and a password hash.
- `IAdminUserService` / `AdminUserService` wraps admin-user use cases in `FluentResults` and uses `IUserDbContext`.
- Users Application creates the first admin for bootstrap through `CreateInitialAdminUserAsync` and validates post-bootstrap admin login through `ValidateAdminCredentialsAsync`.
- API hosts expose Users behavior by calling Users Application services in-process.

## Ownership

Users module owns:

- User and admin-user records.
- Credential hashes, MFA factors, recovery codes, and verification records.
- Account status such as locked, disabled, archived, or verified.
- User lifecycle events such as user created, password changed, user locked, and MFA enabled when messaging is introduced.

Users module does not own:

- Bootstrap/setup state.
- OAuth/OIDC flows, authorization codes, refresh tokens, or sessions.
- OAuth client configuration.
- Signing keys or JWKS.
- Notifications, webhooks, or audit storage.

## Security Rules

- Never return `PasswordHash` or secret-bearing fields through HTTP or message contracts.
- Store password hashes only, never raw passwords.
- Verify password hashes inside Users Application; API hosts should not duplicate password-hash parsing or comparison.
- Add password/MFA/recovery-code flows behind Application services, not controllers directly.
- Avoid account enumeration in external errors.
- Keep credential validation responses deliberately generic at service boundaries.
- Normalize identity fields consistently before enforcing uniqueness.

## Layer Rules

- Domain contains entities and core invariants.
- Application contains user use cases, `IUserDbContext`, and maps exceptions to `FluentResults`.
- Infrastructure contains EF Core DbContext and migrations.
- Avoid repositories by default. Use EF Core directly in Application through `IUserDbContext`; Application services decide when to call `SaveChangesAsync`.
- Keep EF Core attributes/configuration out of transport contracts.

## Data Guidance

- Add User Service migrations in `Veritas.UserService.Infrastructure`.
- `AdminUser.Email` currently has a unique index in `UserDbContext.OnModelCreating`.
- Users tables should use the module-owned `users` schema where possible.
- Keep Users persistence independent from Platform. Platform may ask questions through a consumer-owned Application port implemented by an API host; it must not query `UserDbContext`.

## Verification

From the repo root:

```powershell
dotnet build Veritas.slnx
dotnet run --project src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj
```

When changing Users Application behavior, add/update focused tests in `Veritas.UserService.Application.Tests` and rebuild the solution.
