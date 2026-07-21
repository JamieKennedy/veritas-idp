# Docker Compose deployment

The release bundle runs the complete currently implemented Veritas application:

- Postgres, Redis, and RabbitMQ;
- a one-shot database migrator;
- Admin API;
- Admin UI.

Admin API and the migrator use the same `veritas-backend` image with different commands. This avoids distributing a third image while preserving a clear migration success or failure boundary. A future Public API will be another service using the same backend image and its own `/app/public-api` command.

## Images and commands

Published images are:

- `ghcr.io/jamiekennedy/veritas-backend:<version>`
- `ghcr.io/jamiekennedy/veritas-admin-ui:<version>`
- later, `ghcr.io/jamiekennedy/veritas-public-ui:<version>`

The shared backend image uses `dotnet` as its entry point. Its commands are:

- Admin API, and the image default: `/app/admin-api/Veritas.Admin.API.dll`
- one-shot migrator: `/app/migrator/Veritas.Tooling.DbMigrator.dll`
- later, Public API: `/app/public-api/Veritas.Public.API.dll`

The future Public API remains a separately configured, health-checked, and scalable Compose service even though it reuses the backend image. Admin-only and public HTTP models remain isolated in their respective publish directories. The public UI receives its own image because it has a separate browser bundle and release surface.

## Prerequisites

- Docker Engine with Docker Compose v2.
- An exact Veritas version from a GitHub release or prerelease.
- A trusted HTTPS reverse proxy or load balancer.
- A database backup before every upgrade.

The Compose file publishes only Admin UI, bound to `127.0.0.1` by default. Configure the external proxy to terminate HTTPS and forward to that local port. Do not expose Admin API or the infrastructure services directly.

## Configure

Copy the example environment file:

```powershell
Copy-Item .env.example .env
New-Item -ItemType Directory -Force secrets
```

Set `VERITAS_VERSION` to an exact version such as `0.2.0-beta.3`. Replace both password placeholders with independently generated high-entropy values. Generate the RabbitMQ password from URI-unreserved characters because Compose embeds it in an AMQP URI; 32 random bytes encoded as 64 hexadecimal characters is a safe default.

Generate the bootstrap secret and store only that value in `secrets/bootstrap-secret.txt`. See [Deployment secrets](secrets.md).

The important runtime settings are:

- `VERITAS_VERSION`: immutable backend and Admin UI image tag.
- `VERITAS_BIND_ADDRESS`: host address for Admin UI; defaults to loopback.
- `VERITAS_ADMIN_UI_PORT`: host port forwarded to Admin UI.
- `VERITAS_POSTGRES_PASSWORD`: Postgres application password.
- `VERITAS_RABBITMQ_PASSWORD`: RabbitMQ application password.
- `VERITAS_BOOTSTRAP_SECRET_FILE`: path to the bootstrap-secret file.
- `ADMIN_API_ORIGIN`: internal UI-to-API origin, set by Compose.

## Start and inspect

Pull the exact images and start the stack:

```powershell
docker compose pull
docker compose up -d
docker compose ps -a
```

Expected ordering is:

1. Postgres becomes healthy.
2. The migrator acquires its advisory lock, applies each module migration, and exits with code `0`.
3. Admin API starts and becomes healthy at its internal `/alive` endpoint.
4. Admin UI starts and becomes healthy at `/healthz`.

Inspect startup evidence:

```powershell
docker compose logs migrator
docker compose logs admin-api
docker compose logs admin-ui
```

Do not continue if the migrator exits non-zero, an API repeatedly restarts, or logs contain an unexpected exception.

If migration fails, Compose deliberately leaves Admin API stopped. Preserve the failed migrator logs, correct the database, configuration, or migration defect, take or restore the appropriate backup, and run `docker compose up -d` again. The migrator's advisory lock and idempotent EF migration history make a safe retry possible; do not bypass the migrator by starting the API manually.

## Manual staging acceptance

Use an immutable beta such as `0.2.0-beta.3`, never the moving `beta` tag, when recording acceptance.

1. Start from empty disposable volumes or an appropriate restored test backup.
2. Confirm the migrator exits successfully.
3. Open the HTTPS Admin UI endpoint through the external proxy.
4. Confirm the initial setup or login route renders without browser console failures.
5. Confirm `/api/v1/setup/status` succeeds through the Admin UI origin.
6. Exercise the bootstrap entry flow without recording the bootstrap secret in screenshots or logs.
7. Confirm Admin API, Postgres, Redis, and RabbitMQ have no published host ports.
8. Inspect container status, restarts, and sanitized logs.
9. Record the tested prerelease in the `staging -> main` PR.

The current pipeline does not automate this full-stack acceptance procedure.

## Stop, reset, and upgrade

Stop containers while preserving data:

```powershell
docker compose down
```

`docker compose down -v` permanently removes the Compose-managed database, messaging, Redis, logs, and data-protection volumes. Use it only for an intentional disposable-environment reset.

For an upgrade:

1. Back up Postgres and the data-protection volume.
2. Read the release notes for migration or compatibility warnings.
3. Change `VERITAS_VERSION` to the exact new version.
4. Run `docker compose pull`.
5. Run `docker compose up -d`.
6. Verify migration exit, health, and logs before admitting traffic.

Application rollback is safe only when the older application version is compatible with the migrated schema. Use expand-and-contract database changes for rolling compatibility; restore from backup when a migration itself must be reversed.

## Local image testing

Build the same topology from the repository:

```powershell
Copy-Item .env.example .env
docker compose -f compose.yaml -f compose.local.yaml build
docker compose -f compose.yaml -f compose.local.yaml up -d
```

Aspire remains the preferred development loop. Local Compose validates the production packaging and service boundaries.
