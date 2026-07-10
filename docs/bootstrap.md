# Bootstrap Flow

Bootstrap creates the first administrator account for a new Veritas installation. It is separate from normal admin login and does not send email.

## Prerequisites

- Admin API has either `BOOTSTRAP_SECRET_FILE` or `BOOTSTRAP_SECRET` configured.
- The database migrations have run.
- No administrator account exists yet.
- The client can keep cookies between bootstrap requests.

In Docker Compose, the bootstrap secret is provided from:

```text
/run/secrets/bootstrap_secret
```

The source file defaults to:

```text
./secrets/bootstrap-secret.txt
```

## Status Check

Use the setup status endpoint when a frontend needs to decide which setup screen to show:

```http
GET /api/v1/setup/status
```

Example response:

```json
{
  "isConfigured": false,
  "hasActiveBootstrap": false,
  "activeBootstrapExpiresAtUtc": null,
  "isSmtpConfigured": false
}
```

`isConfigured` becomes `true` once at least one admin account exists and bootstrap has completed.

The Admin UI uses this state to select its first route:

- Not configured, no active bootstrap: `/bootstrap/start`.
- Not configured, active bootstrap: `/bootstrap/complete`.
- Configured, no authenticated admin: `/login`.
- Configured, authenticated admin: `/dashboard`.

Bootstrap routes are unavailable after setup is configured. Authenticated routes redirect to login when the admin cookie is missing or invalid, and login redirects authenticated administrators to the dashboard.

## Start Bootstrap

Start creates a short-lived bootstrap session after validating the deployment bootstrap secret.

```http
POST /api/v1/bootstrap/start
Content-Type: application/json
```

```json
{
  "email": "admin@example.com",
  "bootstrapSecret": "<deployment-bootstrap-secret>"
}
```

Success returns `202 Accepted` and sets:

```text
__Host-veritas-bootstrap
```

Cookie properties:

- `HttpOnly`
- `Secure`
- `SameSite=Strict`
- `Path=/`
- `Max-Age=15 minutes`

The raw bootstrap session token is stored only in this cookie. The server stores a hash of the token.

## Complete Bootstrap

Complete creates the first admin password using the active bootstrap cookie.

```http
POST /api/v1/bootstrap/complete
Content-Type: application/json
Cookie: __Host-veritas-bootstrap=<cookie-value>
```

```json
{
  "password": "<first-admin-password>",
  "displayName": "First Admin"
}
```

Success returns `200 OK` and clears the bootstrap cookie.

## After Bootstrap

Bootstrap does not issue the normal admin auth cookie. After bootstrap completes, the frontend should move to the normal admin login flow.

With the MFA-backed admin auth flow, the first password login for the new admin returns an MFA enrollment challenge. The admin must enroll TOTP before the admin auth cookie is issued.

After the first authenticated login, the Admin UI opens `/bootstrap/smtp`. SMTP can be skipped after confirming a warning. Skipping does not mark SMTP as configured; `/dashboard` continues to show an email-delivery warning linked to `/bootstrap/smtp` until a connection test succeeds.

## Common Failures

- `400 Bad Request`: missing first-admin email, bootstrap secret, or password.
- `401 Unauthorized`: bootstrap secret or bootstrap session token is invalid.
- `409 Conflict`: bootstrap is already configured, another active bootstrap session exists, or the active session expired.
- Missing cookie on complete: the client did not preserve `__Host-veritas-bootstrap`, the cookie expired, or the request is not using HTTPS.

## Security Notes

- Do not log, display, or persist the bootstrap secret in frontend storage.
- Do not send the bootstrap secret through messaging or email.
- Rotate the deployment bootstrap secret after initial setup if operational policy requires it.
- Deleting the bootstrap-completed flag alone does not safely reopen setup; Veritas also treats the existence of an admin account as configured state.
