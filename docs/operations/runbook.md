---
title: Operations and recovery
description: Monitor health, inspect logs, back up state, upgrade safely, recover failures, and reset disposable Veritas deployments.
section: Deploy and operate
sectionOrder: 1
order: 4
---

This runbook complements the [Docker Compose deployment guide](./docker-compose.md). Record the exact Veritas version, Compose bundle checksum, host configuration revision, and backup used for every operational change.

## Health and startup

Inspect the complete service state:

<div data-shell-tabs="inspect-services"></div>

```bash
docker compose ps -a
```

```powershell
docker compose ps -a
```

Expected startup order:

1. PostgreSQL and RabbitMQ become healthy.
2. The migrator acquires its PostgreSQL advisory lock, applies module migrations, and exits with code `0`.
3. Admin API starts and passes its internal `/alive` check.
4. Admin UI starts and passes `/healthz`.

Inside the Admin API container, `/alive` checks process liveness. `/health` checks PostgreSQL readiness and returns HTTP 503 when the database is unavailable. Health responses contain aggregate status only.

Admin API completion events use route templates and include a trace ID when tracing is active. Use the `traceId` from an unexpected-error response to locate the corresponding failure in the Admin API log.

The Admin API and migrator each write to their own rolling file in the mounted `/var/log/veritas` volume. Configure distinct file paths for processes that share a log directory.

Admin API intentionally exposes only process liveness in production. Dependency-rich readiness details remain development-only to avoid disclosing internal topology.

## Logs and failure evidence

<div data-shell-tabs="inspect-logs"></div>

```bash
docker compose logs --since 30m migrator
docker compose logs --since 30m admin-api
docker compose logs --since 30m admin-ui
docker compose events --since 30m
```

```powershell
docker compose logs --since 30m migrator
docker compose logs --since 30m admin-api
docker compose logs --since 30m admin-ui
docker compose events --since 30m
```

Preserve timestamps, container status, restart counts, the exact version, and sanitized error context. Remove credentials, tokens, hashes, personal data, connection strings, and rendered email content before sharing evidence.

Do not bypass a failed migrator by manually starting Admin API. Correct the dependency, configuration, or migration issue and retry the normal Compose startup so the ordering and migration lock remain effective.

## Backups

Back up at least:

- PostgreSQL using a tested logical or physical backup procedure;
- the `veritas-data-protection` volume;
- reviewed deployment configuration and reverse-proxy configuration;
- certificate material according to the proxy's recovery model; and
- a sanitized record of deployed image tags and Compose bundle checksums.

RabbitMQ and Redis volumes support current runtime behavior but are not substitutes for PostgreSQL or Data Protection backups. Losing Data Protection keys can invalidate protected authentication state and make protected configuration unreadable. Exposing those keys weakens the protection of every payload encrypted with them.

Test restoration on an isolated host. A backup that has never been restored is not verified recovery evidence.

## Upgrade

1. Read the release notes and compatibility warnings.
2. Verify the new Compose bundle checksum.
3. Back up PostgreSQL and Data Protection keys.
4. Change `VERITAS_VERSION` to the exact new immutable version.
5. Run `docker compose config --quiet`.
6. Pull images with `docker compose pull`.
7. Start with `docker compose up -d`.
8. Verify migrator exit, service health, restart counts, logs, bootstrap state, administrator login, and SMTP status.

Do not admit traffic until verification succeeds.

## Rollback

Application rollback is safe only when the older application understands the migrated schema. Check the release notes before changing the version back.

If a database migration itself must be reversed, restore a compatible PostgreSQL backup and the matching Data Protection state rather than editing EF migration history manually. Preserve failed-state evidence before recovery.

## Dependency failures

- **PostgreSQL unavailable:** migrator retries for its configured bounded period; Admin API remains stopped.
- **RabbitMQ unavailable:** Admin API waits for RabbitMQ health in the supplied Compose topology.
- **Admin API unhealthy:** Admin UI remains blocked by its dependency condition during initial startup.
- **SMTP unavailable:** the submitted SMTP configuration is not persisted unless its connectivity test succeeds.
- **Proxy or certificate failure:** keep external traffic blocked until trusted HTTPS is restored.

Investigate the first failing dependency rather than repeatedly restarting the entire stack.

## Stop and reset

Stop while preserving state:

<div data-shell-tabs="stop-services"></div>

```bash
docker compose down
```

```powershell
docker compose down
```

`docker compose down -v` permanently removes Compose-managed PostgreSQL, RabbitMQ, Redis, Data Protection, and log volumes. Use it only for a confirmed disposable-environment reset after verifying the Compose project name and target host.

For retained environments, restore from backup or repair the specific dependency. Never delete database or key volumes as a general troubleshooting step.
