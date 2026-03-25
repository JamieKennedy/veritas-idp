# Setup Token

This document explains how the setup token works in Veritas, how to provide it in local Aspire development, and how to handle it in deployments.

## What it is

The setup token is a shared secret used to authorize the initial setup request.

- In `src/orchestration/Veritas.AppHost/AppHost.cs`, AppHost defines a secret parameter:
  - `builder.AddParameter("setup-token", secret: true)`
- AppHost injects that value into the Admin API container/process as:
  - `SETUP_TOKEN`
- In `src/backend/Veritas.Admin.API/Controllers/v1/SetupController.cs`, setup calls are accepted only when `setupDto.SetupToken` matches `SETUP_TOKEN`.

## Local development (Aspire)

Because this value is a secret parameter, you should provide it via user-secrets (recommended) or environment variable.

### Option 1: user-secrets (recommended)

Run these commands from anywhere:

```powershell
dotnet user-secrets set "Parameters:setup-token" "your-dev-setup-token" --project "C:\Users\jamie\Documents\Projects\veritas\src\orchestration\Veritas.AppHost\Veritas.AppHost.csproj"
dotnet run --project "C:\Users\jamie\Documents\Projects\veritas\src\orchestration\Veritas.AppHost\Veritas.AppHost.csproj"
```

Notes:

- `Veritas.AppHost.csproj` already contains a `UserSecretsId`, so no extra initialization is required.
- Do not commit token values to source control.

### Option 2: environment variable

```powershell
$env:Parameters__setup-token = "your-dev-setup-token"
dotnet run --project "C:\Users\jamie\Documents\Projects\veritas\src\orchestration\Veritas.AppHost\Veritas.AppHost.csproj"
```

## Deployment guidance

When deploying outside Aspire local orchestration (for example, Docker Compose or cloud environments), deployment should provide the token.

Recommended sources:

- Secret manager (for example, Azure Key Vault, AWS Secrets Manager, 1Password Connect)
- CI/CD secret variables
- Docker/Kubernetes secrets

Avoid:

- Printing `SETUP_TOKEN` in logs
- Committing token values to `appsettings*.json`, Compose files, or source code

## Rotation and lifecycle

- Treat the setup token as short-lived bootstrap credentials.
- Rotate or invalidate it after initial setup is complete.
- If setup is not complete, rotate by updating the secret source and restarting/redeploying services that consume it.

## Troubleshooting

### `SETUP_TOKEN is not configured`

This means the Admin API did not receive the token.

Check:

- AppHost is running and the `setup-token` parameter is set.
- The parameter key is exactly `Parameters:setup-token` (user-secrets) or `Parameters__setup-token` (environment variable).
- The Admin API is started by AppHost and not separately without env injection.

### Setup returns `Unauthorized("Invalid setup token.")`

Check:

- The value sent in `setupDto.SetupToken` exactly matches the configured token.
- No extra whitespace/newline characters are present.

