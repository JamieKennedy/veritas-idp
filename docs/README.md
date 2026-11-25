---
title: Veritas documentation
description: Deploy, configure, operate, secure, and contribute to the Veritas identity provider.
section: Start here
sectionOrder: 0
order: 0
label: Documentation overview
---

Veritas is an experimental, self-hostable authentication service and identity provider. The current implementation provides a secured first-run administration flow, mandatory administrator MFA, SMTP configuration, module-owned persistence, and production-shaped Docker Compose packaging.

## Choose a path

- **Deploy Veritas:** start with the [Docker Compose guide](./operations/docker-compose.md), then configure [environment variables](./operations/environment.mdx), [secrets](./operations/secrets.md), and your [HTTPS reverse proxy](./operations/reverse-proxy.md).
- **Administer an instance:** follow the [bootstrap flow](./bootstrap.md), understand [administrator authentication](./admin-auth-flow.md), and configure [SMTP and messaging](./smtp-messaging.md).
- **Operate safely:** use the [operations and recovery runbook](./operations/runbook.md) for health checks, logs, backups, upgrades, rollback, and failure handling.
- **Contribute:** read the [architecture guide](./engineering/architecture.md), [coding standards](./engineering/coding-standards.md), and [branching and release process](./engineering/branching-and-releases.md).

## Current scope

The documentation describes the currently implemented Admin API and Admin UI. OAuth 2.0, OpenID Connect, public-user authentication, client management, signing-key management, federation, and production support guarantees are not implemented.

Examples deliberately use placeholders. Never paste real passwords, tokens, one-time codes, or private deployment details into issues, screenshots, logs, or documentation.

## Project policies

- [Report a security vulnerability](https://github.com/JamieKennedy/veritas-idp/security/policy)
- [Contribute to Veritas](https://github.com/JamieKennedy/veritas-idp/blob/staging/CONTRIBUTING.md)
- [Read the support policy](https://github.com/JamieKennedy/veritas-idp/blob/main/SUPPORT.md)
- [Review the code of conduct](https://github.com/JamieKennedy/veritas-idp/blob/main/CODE_OF_CONDUCT.md)
