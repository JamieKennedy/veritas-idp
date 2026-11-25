---
title: Getting started
description: Choose between deploying a release with Docker Compose and running the Veritas source tree with Aspire.
section: Start here
sectionOrder: 0
order: 1
---

Veritas has two supported entry points: deploy an immutable release bundle with Docker Compose, or run the source tree with .NET Aspire for development. Do not use the Aspire topology as a production deployment.

## Before you begin

Veritas currently requires:

- a host capable of running Docker Engine and Docker Compose v2 for deployments;
- an HTTPS reverse proxy or load balancer for every remotely accessible instance;
- enough persistent storage for PostgreSQL, RabbitMQ, Redis, Data Protection keys, and logs;
- an explicit backup and recovery policy; and
- acceptance that pre-alpha releases may contain breaking changes and do not carry production support guarantees.

## Deploy a release

Use an exact stable or prerelease version from the [GitHub Releases page](https://github.com/JamieKennedy/veritas-idp/releases). Download the Compose bundle attached to that release, verify its checksum, and follow the [Docker Compose deployment guide](./operations/docker-compose.md).

The deployment flow is:

1. Create `.env` from the supplied `.env.example`.
2. Generate independent PostgreSQL, RabbitMQ, and bootstrap secrets.
3. Validate the resolved Compose configuration.
4. Pull the exact release images.
5. Start the stack and wait for the one-shot migrator to succeed.
6. Put the Admin UI behind trusted HTTPS.
7. Complete bootstrap, administrator MFA enrollment, and SMTP setup.

Continue with:

- [Environment variables](./operations/environment.mdx)
- [Secret management](./operations/secrets.md)
- [Reverse proxy and HTTPS](./operations/reverse-proxy.md)
- [Operations and recovery](./operations/runbook.md)

## Run the source tree

Local development requires .NET 11, Node.js 22, pnpm 10.15.1, Docker, and Aspire CLI 17.0.0.

<div data-shell-tabs="local-development"></div>

```bash
pnpm restore
dotnet user-secrets --project src/orchestration/aspire/Veritas.AppHost set "Parameters:bootstrap-secret" "<generated-development-secret>"
pnpm dev
```

```powershell
pnpm restore
dotnet user-secrets --project src/orchestration/aspire/Veritas.AppHost set "Parameters:bootstrap-secret" "<generated-development-secret>"
pnpm dev
```

Aspire starts PostgreSQL, Redis, RabbitMQ, the database migrator, Admin API, and Admin UI. Use the Aspire dashboard links rather than assuming individual ports, and never reuse the development bootstrap secret in a deployment.

To work on a single frontend:

<div data-shell-tabs="frontend-development"></div>

```bash
pnpm dev:admin-ui
pnpm dev:docs
```

```powershell
pnpm dev:admin-ui
pnpm dev:docs
```

Before contributing, read the [architecture](./engineering/architecture.md), [coding standards](./engineering/coding-standards.md), and [branching and releases](./engineering/branching-and-releases.md) guides.

## First administration flow

After the deployment is healthy:

1. [Bootstrap the first administrator](./bootstrap.md).
2. Sign in and complete mandatory [TOTP MFA enrollment](./admin-auth-flow.md).
3. Save the one-time recovery codes outside the Veritas host.
4. Configure or explicitly defer [SMTP setup](./smtp-messaging.md).

Never store bootstrap secrets, passwords, TOTP secrets, recovery codes, session values, or SMTP credentials in browser storage, screenshots, shell history, or support requests.
