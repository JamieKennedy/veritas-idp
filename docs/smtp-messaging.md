# SMTP And Messaging Setup

SMTP setup configures outbound email delivery for Veritas messaging workflows. It happens after bootstrap and admin MFA login.

## Setup Gate

Some protected admin workflows are blocked until SMTP setup succeeds. The setup status endpoint includes both bootstrap and SMTP state:

```http
GET /api/v1/setup/status
```

Example:

```json
{
    "isConfigured": true,
    "hasActiveBootstrap": false,
    "activeBootstrapExpiresAtUtc": null,
    "isSmtpConfigured": false
}
```

When an authenticated admin calls a protected workflow before SMTP is configured, the API may return `428 Precondition Required`.

## Authentication Requirements

Messaging endpoints require:

- A valid admin auth cookie, `__Host-veritas-admin`.
- A valid CSRF token header, `X-CSRF-TOKEN`, for unsafe methods such as `PUT`.
- SMTP setup itself is allowed before SMTP is configured, but the caller must still be authenticated.

See [Admin authentication flow](./admin-auth-flow.md) for login, MFA, cookies, and CSRF.

## Read SMTP Settings

```http
GET /api/v1/messaging/settings
Cookie: __Host-veritas-admin=<cookie-value>
```

The response never includes the raw SMTP secret. It uses `hasSecret` to indicate whether a secret is stored.

Example:

```json
{
    "host": "smtp.example.com",
    "port": 587,
    "tlsMode": "StartTls",
    "username": "smtp-user",
    "hasSecret": true,
    "fromEmail": "no-reply@example.com",
    "fromName": "Veritas",
    "isConfigured": true,
    "lastSuccessfulTestAtUtc": "2026-06-12T10:00:00Z"
}
```

If SMTP is not configured, the API returns a controlled problem response with the messaging not-configured error.

## Configure SMTP

Configuration validates the request, performs a live test send, and persists settings only after the test succeeds.

```http
PUT /api/v1/messaging/settings/smtp
X-CSRF-TOKEN: <csrf-token>
Cookie: __Host-veritas-admin=<cookie-value>; __Host-veritas-admin-csrf=<cookie-value>
Content-Type: application/json
```

```json
{
    "host": "smtp.example.com",
    "port": 587,
    "tlsMode": "StartTls",
    "username": "smtp-user",
    "secret": "<smtp-password-or-token>",
    "fromEmail": "no-reply@example.com",
    "fromName": "Veritas"
}
```

Validation rules:

- `host` is required and trimmed.
- `port` must be between `1` and `65535`.
- `tlsMode` must be a valid SMTP TLS mode.
- `fromEmail` must be a valid email address and is normalized to lowercase.
- `username` and `fromName` are optional and trimmed.
- `secret` is optional; when present, it is protected before persistence.

Success returns the safe SMTP settings projection. It does not return the raw secret.

## Email Templates

List templates:

```http
GET /api/v1/messaging/templates
```

Get a template:

```http
GET /api/v1/messaging/templates/{templateKey}
```

Update a global template:

```http
PUT /api/v1/messaging/templates/{templateKey}
X-CSRF-TOKEN: <csrf-token>
Content-Type: application/json
```

```json
{
    "subject": "Subject text",
    "htmlBody": "<p>Hello {{User.DisplayName}}</p>",
    "textBody": "Hello {{User.DisplayName}}",
    "isEnabled": true
}
```

## Security Notes

- Never log SMTP secrets, protected secret payloads, rendered email bodies, OTPs, setup tokens, passwords, session tokens, or client secrets.
- Treat SMTP configuration as a privileged admin workflow.
- A failed test send does not persist the submitted settings.
- The current messaging setup uses one installation-level SMTP settings record.
