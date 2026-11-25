---
title: Secret management
description: Generate, store, back up, rotate, and recover Veritas deployment secrets safely.
section: Deploy and operate
sectionOrder: 1
order: 2
---

Never commit `.env`, the `secrets` directory, bootstrap secrets, database or RabbitMQ passwords, signing keys, SMTP credentials, recovery codes, or Data Protection keys.

## Secret inventory

| Material                     | Current location                                     | Required handling                                                                                     |
| ---------------------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| PostgreSQL password          | `.env`, interpolated into Compose                    | Independent high-entropy value; restrict the file and redact resolved Compose output.                 |
| RabbitMQ password            | `.env`, interpolated into an AMQP URI                | Use URI-unreserved characters; 64 hexadecimal characters is the recommended default.                  |
| Bootstrap secret             | Host file mounted at `/run/secrets/bootstrap_secret` | Generate independently, restrict to the deployment account, and never place it in browser storage.    |
| Data Protection keys         | `veritas-data-protection` volume                     | Persist, restrict, and back up with the database; compromise can expose protected application values. |
| SMTP password or token       | Submitted through Admin UI and protected at rest     | Use a provider-scoped credential; never expect the read API to return it.                             |
| Administrator recovery codes | Displayed once after MFA enrollment                  | Store outside the Veritas host; the server retains only hashes and each code is one-time use.         |

Docker Compose currently mounts the bootstrap secret as a service-scoped file. PostgreSQL and RabbitMQ passwords remain Compose interpolation inputs in `.env`; do not claim they receive file-mounted secret handling in the supplied topology.

## Generate values

Generate each credential independently. Do not derive one from another or reuse values across environments.

Generate the values with OpenSSL:

<div data-shell-tabs="generate-secrets"></div>

```bash
umask 077
mkdir -p secrets
openssl rand -hex 32
openssl rand -hex 32
openssl rand -base64 48 > secrets/bootstrap-secret.txt
chmod 600 .env secrets/bootstrap-secret.txt
```

```powershell
New-Item -ItemType Directory -Force secrets
openssl rand -hex 32
openssl rand -hex 32
openssl rand -base64 48 | Set-Content -NoNewline secrets/bootstrap-secret.txt
```

Use the two hexadecimal outputs for `VERITAS_POSTGRES_PASSWORD` and `VERITAS_RABBITMQ_PASSWORD`. Write only the base64 output to `secrets/bootstrap-secret.txt`.

On Windows, restrict `.env` and the `secrets` directory with NTFS ACLs so only the deployment service account and approved administrators can read them. Do not rely on repository ignore rules as access control.

## Bootstrap-secret lifecycle

The bootstrap secret is accepted only by the narrow first-administrator setup flow. Admin API reads the file at startup, trims surrounding whitespace, and fails startup when the configured file is missing or empty.

- Generate it before the first startup.
- Enter it only in the HTTPS bootstrap form.
- Do not put it in a URL, shell argument, screenshot, ticket, or log.
- After bootstrap, remove unnecessary copies according to the recovery policy.
- Rotate it after suspected exposure even though completed bootstrap cannot be reopened by the secret alone.

Deleting the bootstrap-completed flag does not safely reopen setup. Veritas also treats the existence of an administrator account as configured state.

## Data Protection keys

Veritas protects retrievable secrets and authentication state with ASP.NET Core Data Protection. The Compose deployment persists keys in `veritas-data-protection`.

Back up this volume alongside PostgreSQL:

- Losing the keys can invalidate cookies and make protected configuration unreadable.
- Restoring a database without compatible keys can leave encrypted data unusable.
- Exposing the keys weakens every value protected with them.
- Copying one key ring between unrelated environments crosses a security boundary and is not supported.

Keep backup encryption keys and access policy separate from the Veritas host.

## Rotation

Rotate one dependency at a time and preserve a recovery point.

1. Take and verify the required backups.
2. Change the credential at its source of truth.
3. Update the protected deployment configuration.
4. Restart only the affected services through Compose.
5. Verify migrator/API health, authentication, messaging, and sanitized logs.
6. Revoke the old credential after the new value is confirmed.

Database and RabbitMQ rotation may require coordinated server-side and application configuration changes. Do not simply replace `.env` while the dependency still expects the old password.

SMTP rotation is performed by submitting and successfully testing the replacement credential in Admin UI. Failed connectivity tests do not persist the submitted settings.

## Suspected exposure

Treat a secret as compromised when it appears in source control, shell history, process arguments, CI output, terminal recordings, screenshots, support requests, telemetry, or logs.

1. Stop further disclosure and preserve sanitized evidence.
2. Revoke or rotate the exposed value.
3. Invalidate dependent sessions or state where relevant.
4. Search logs, artifacts, backups, and reachable Git history for additional copies.
5. Remove public copies only after rotation; deletion is not revocation.
6. Record the incident without reproducing the secret.

Use the repository's private vulnerability-reporting process for security-sensitive exposure. Never open a public issue containing a real credential.
