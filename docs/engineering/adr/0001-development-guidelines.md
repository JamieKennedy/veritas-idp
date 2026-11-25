---
title: Adopt the development guidelines
description: How Veritas applies the shared development guidelines to its architecture and delivery workflow.
section: Development
sectionOrder: 4
order: 3
draft: true
---

## Context

The [development guidelines](../development-guidelines.md) were written for applications that commonly run on IIS and SQL Server. Veritas is a self-hostable identity provider with a PostgreSQL database and Linux container deployment. This decision covers the repository and is owned by the Veritas maintainers.

## Decision

Apply the shared guidance alongside the [coding standards](../coding-standards.md). Use the decisions below when a shared default differs from Veritas's architecture or tooling. The checked-in guide is the reference edition for this adoption.

The adoption review excludes SQL Server, SQL-first EF, self-hosted runners, Azure DevOps, and the guide's reference implementations.

| Scope                        | Veritas decision and reason                                                                                                                                                                                                                                                                                       | Review trigger                                                                 |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| Backend projects             | Use Domain, Application and Infrastructure per module, with composition in separate API hosts. Module-owned EF Core context interfaces let use cases control atomic writes. Consumer-owned ports preserve ownership across modules.                                                                               | Module extraction or a persistence boundary change.                            |
| Schema ownership             | Use one PostgreSQL database with module-owned schemas and EF Core migrations. The one-shot migrator applies changes before the API starts, and the owning module maintains its mappings. This supports container-based self-hosting.                                                                              | A new storage provider or a change to the upgrade model.                       |
| Hosting and releases         | Use Aspire locally, Docker Compose for deployment, and GitHub Actions for verification and publication. Versioned images and the Compose bundle are the promotion units. Follow the [branching and release process](../branching-and-releases.md) and [Docker Compose guide](../../operations/docker-compose.md). | A new supported deployment target or CI platform.                              |
| Browser and machine identity | Preserve Veritas's administrator password and MFA flow, host-only Secure cookies with `SameSite=Strict`, and same-origin production API. The separate Admin UI origin is a local development convenience. Evaluate Entra, federation and machine credentials when their audiences and contracts are defined.      | An external identity provider, machine endpoint or new audience is introduced. |
| Frontend structure           | Keep TanStack Start file routing and capability folders. Use the checked-in Prettier configuration and installed UI primitives. Apply TanStack Table and additional shadcn components where the feature requires them.                                                                                            | A router or UI foundation upgrade.                                             |
| C# documentation             | Use XML documentation for public contracts and for internal behavior with non-obvious constraints. This keeps guarantees near the entry points and avoids restating straightforward private helpers and test names.                                                                                               | Public SDK publication or a documentation regression in a supported API.       |
| Documentation layout         | Keep feature, operations and engineering docs under the existing lowercase `docs/` tree. The supplied guide is a repository reference with its source examples preserved.                                                                                                                                         | A documentation layout change or a new edition of the supplied guide.          |

## Adoption verification

Use `pnpm check` and `pnpm build` for repository verification. Use `pnpm check:containers` and the Docker Compose acceptance procedure when deployment behavior changes.

The Admin API build emits the v1 OpenAPI document. Regenerate the checked-in Admin UI types with `pnpm generate:api`. CI verifies that the generated types are current and compares the contract with the latest release that includes a v1 artifact. The first release with an artifact establishes the comparison baseline.

Run `pnpm check:admin-workflow` with Docker and Playwright Chromium available to verify first-admin setup, MFA enrollment, sign-out, and sign-in on an isolated Compose installation behind a disposable HTTPS proxy. The same check reruns the migrator on the configured PostgreSQL database and verifies readiness during a database outage. Historical release-to-release upgrade rehearsal remains part of release acceptance.
