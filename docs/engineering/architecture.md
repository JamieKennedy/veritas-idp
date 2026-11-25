---
title: Architecture
description: Understand Veritas module boundaries, onion architecture, runtime composition, persistence, messaging, and deployment topology.
section: Development
sectionOrder: 4
order: 0
---

Veritas is a .NET 11 modular monolith with separate HTTP hosts for different audiences. Modules are composed in-process at an API edge, use one PostgreSQL database with module-owned schemas, and communicate asynchronously through versioned Wolverine/RabbitMQ messages when work should not block the caller.

## Runtime shape

| Component              | Responsibility                                                                                      |
| ---------------------- | --------------------------------------------------------------------------------------------------- |
| Admin UI               | React administration experience and same-origin proxy to Admin API.                                 |
| Admin API              | HTTP validation, authentication, versioning, module composition, failure mapping, and OpenAPI.      |
| Platform module        | Global platform state, bootstrap/setup state, system flags, and instance-level configuration.       |
| Users module           | Administrator identity, credentials, MFA, recovery codes, and server-side sessions.                 |
| Messaging module       | SMTP configuration, protected SMTP secrets, templates, rendering, and email delivery.               |
| Database migrator      | One-shot, ordered EF Core migration for every module-owned DbContext.                               |
| PostgreSQL             | Source of truth, separated by module-owned schemas and migration histories.                         |
| RabbitMQ and Wolverine | Durable asynchronous side effects and cross-module reactions using safe versioned contracts.        |
| Redis                  | Cache and ephemeral state only; never the source of truth for identities, sessions, setup, or keys. |

The future Public API and Public UI will be separate edge surfaces. They will compose the same internal modules without sharing administrator-only controllers or transport models.

## Onion architecture

Each business module follows the same inward dependency direction:

1. **Domain** contains entities, value objects, enums, domain errors, and core rules.
2. **Application** contains use cases, DTOs, orchestration, module-owned persistence interfaces, and consumer-owned ports.
3. **Infrastructure** implements persistence and external adapters.
4. **API hosts** compose modules and translate HTTP requests and responses.

Domain does not reference infrastructure or transport frameworks. Application code can use EF Core through its own DbContext interface. Infrastructure references Application and Domain to implement those interfaces. API hosts reference Application and Infrastructure for composition.

## Module boundaries

- A module owns its tables, DbContext, migrations, entities, and write model.
- One module never injects or queries another module's DbContext.
- Immediate cross-module validation uses a consumer-owned Application port implemented at composition.
- Durable side effects use versioned contracts under `Veritas.Contracts.Messages`.
- Async contracts contain safe payloads only and never raw passwords, tokens, OTPs, signing keys, SMTP secrets, or recovery codes.
- Cross-module database foreign keys are avoided; identifiers are stored only when integration requires them.

## Persistence and startup

Platform, Users, and Messaging use separate DbContexts and migration history tables in one PostgreSQL database. The migrator runs before Admin API in both Aspire and Docker Compose.

The migrator uses a PostgreSQL advisory lock, bounded connection retries, and EF migration histories. A failed migration prevents API startup. Use expand-and-contract schema changes when compatibility across versions is required.

## Security boundaries

The browser and Admin UI are untrusted inputs. Admin API validates transport shape, credentialed CORS, CSRF, cookies, rate limits, authentication, and authorization before calling module use cases.

Secrets are either hashed when only comparison is needed or protected with persistent ASP.NET Core Data Protection keys when retrieval is required. Logs and durable messages must not contain raw credentials or protected payloads.

Read the [threat model](../security/threat-model.md) before changing an externally reachable endpoint, credential type, trust boundary, privileged dependency, persistent secret, authentication method, or deployment topology.

## Local and deployment orchestration

.NET Aspire is the preferred development loop. It injects development configuration and starts local dependencies with observable wait relationships.

Docker Compose is the intended self-hosted release shape. It publishes only Admin UI, keeps infrastructure on an internal network, runs migrations as a one-shot dependency, uses read-only application containers, and expects HTTPS termination at an operator-managed reverse proxy.
