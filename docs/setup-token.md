# Bootstrap Secret

This document explains how the bootstrap secret works in Veritas, how to provide it in local Aspire development, and how to handle it in deployments.

## What It Is

The bootstrap secret is a deployment-owned shared secret used only to start first-admin bootstrap.

- In `src/orchestration/aspire/Veritas.AppHost/AppHost.cs`, AppHost defines a secret parameter:
  - `builder.AddParameter("bootstrap-secret", secret: true)`
- AppHost injects that value into the Admin API container/process as:
  - `BOOTSTRAP_SECRET`
- Outside Aspire, Admin API resolves configuration in this order:
  - `BOOTSTRAP_SECRET_FILE`
  - `BOOTSTRAP_SECRET`

Bootstrap does not send email. The operator supplies the first-admin email address and bootstrap secret to start setup, then completes setup by creating the first-admin password through the short-lived bootstrap cookie.

## Local Development

Because this value is a secret parameter, provide it via user-secrets or an environment variable.

### Option 1: User Secrets

```powershell
dotnet user-secrets set "Parameters:bootstrap-secret" "your-dev-bootstrap-secret" --project "<path-to-your-project>\src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj"
dotnet run --project "<path-to-your-project>\src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj"
```

### Option 2: Environment Variable

```powershell
$env:Parameters__bootstrap-secret = "your-dev-bootstrap-secret"
dotnet run --project "<path-to-your-project>\src\orchestration\aspire\Veritas.AppHost\Veritas.AppHost.csproj"
```

## Deployment Guidance

For Docker Compose or other self-hosted deployments, prefer a mounted secret file:

- Set `BOOTSTRAP_SECRET_FILE` to the mounted file path.
- Use `BOOTSTRAP_SECRET` only as a simpler fallback.

The repository `docker-compose.yml` expects:

- `secrets/bootstrap-secret.txt` containing the bootstrap secret.
- `VERITAS_POSTGRES_PASSWORD` set in the shell or `.env` file.
- `VERITAS_RABBITMQ_PASSWORD` set in the shell or `.env` file.

Example:

```powershell
New-Item -ItemType Directory -Force secrets
Set-Content -NoNewline -Path secrets/bootstrap-secret.txt -Value "your-bootstrap-secret"
$env:VERITAS_POSTGRES_PASSWORD = "change-this-postgres-password"
$env:VERITAS_RABBITMQ_PASSWORD = "change-this-rabbitmq-password"
docker compose up --build
```

Avoid:

- Committing bootstrap secret values.
- Printing bootstrap secrets in logs.
- Sending bootstrap secrets through email, messages, traces, or metrics.

## Rotation And Lifecycle

- Treat the bootstrap secret as sensitive bootstrap credentials.
- Rotate it after initial setup is complete and keep a configured secret source for Admin API startup.
- If setup is not complete, rotate by updating the secret source and restarting/redeploying services that consume it.
- Deleting `BOOTSTRAP_COMPLETED` alone does not reopen setup. Veritas considers the installation configured when at least one administrator user exists.

## Troubleshooting

### `BOOTSTRAP_SECRET_FILE or BOOTSTRAP_SECRET must be configured`

Admin API did not receive a bootstrap secret source.

Check:

- AppHost is running and the `bootstrap-secret` parameter is set.
- The local parameter key is exactly `Parameters:bootstrap-secret` or `Parameters__bootstrap-secret`.
- In deployment, `BOOTSTRAP_SECRET_FILE` points to a non-empty file or `BOOTSTRAP_SECRET` is set.

### Bootstrap Start Returns Unauthorized

Check:

- The value sent as `bootstrapSecret` exactly matches the configured secret.
- No extra whitespace/newline characters are present.
