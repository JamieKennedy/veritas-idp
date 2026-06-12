# Admin Authentication Flow

Admin authentication uses password validation, mandatory TOTP MFA, server-side admin sessions, cookie authentication, and CSRF protection.

Password login alone does not issue the admin auth cookie. The cookie is issued only after MFA enrollment or MFA verification succeeds.

## Cookies And Headers

Admin auth cookie:

```text
__Host-veritas-admin
```

CSRF cookie:

```text
__Host-veritas-admin-csrf
```

CSRF request header:

```text
X-CSRF-TOKEN
```

Use HTTPS. The cookies are configured with `Secure`, so plain HTTP testing will not behave like production.

## Get CSRF Token

Call this before unsafe requests such as login, MFA completion, logout, and SMTP updates:

```http
GET /api/v1/admin-auth/csrf
```

Example response:

```json
{
  "token": "<csrf-token>"
}
```

The frontend should send the returned token as:

```http
X-CSRF-TOKEN: <csrf-token>
```

The browser or API client must also keep the `__Host-veritas-admin-csrf` cookie.

## Start Login

```http
POST /api/v1/admin-auth/login
X-CSRF-TOKEN: <csrf-token>
Content-Type: application/json
```

```json
{
  "email": "admin@example.com",
  "password": "<admin-password>"
}
```

Success returns `202 Accepted` with a short-lived MFA challenge.

First login after bootstrap:

```json
{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "challengeToken": "<challenge-token>",
  "purpose": "MfaEnrollment",
  "expiresAtUtc": "2026-06-12T10:05:00Z",
  "totpSecretBase32": "<base32-secret>",
  "totpProvisioningUri": "otpauth://totp/..."
}
```

Future logins:

```json
{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "challengeToken": "<challenge-token>",
  "purpose": "MfaVerification",
  "expiresAtUtc": "2026-06-12T10:05:00Z",
  "totpSecretBase32": null,
  "totpProvisioningUri": null
}
```

The challenge expires quickly and must be completed with the same `challengeId` and `challengeToken`.

## First Login MFA Enrollment

When `purpose` is `MfaEnrollment`, show TOTP setup UI:

- Render a QR code from `totpProvisioningUri`, or show `totpSecretBase32` for manual entry.
- Ask the admin to add it to an authenticator app.
- Ask for the current 6-digit TOTP code.

Then call:

```http
POST /api/v1/admin-auth/mfa/enroll/confirm
X-CSRF-TOKEN: <csrf-token>
Content-Type: application/json
```

```json
{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "challengeToken": "<challenge-token>",
  "totpCode": "123456"
}
```

Success returns `200 OK`, sets `__Host-veritas-admin`, and returns one-time recovery codes:

```json
{
  "admin": {
    "id": "00000000-0000-0000-0000-000000000000",
    "email": "admin@example.com",
    "name": "First Admin"
  },
  "recoveryCodes": [
    "veritas-example-code"
  ]
}
```

Show recovery codes once and tell the admin to save them. The server stores only hashes.

## Future MFA Verification

When `purpose` is `MfaVerification`, ask for a TOTP code or an unused recovery code.

```http
POST /api/v1/admin-auth/mfa/verify
X-CSRF-TOKEN: <csrf-token>
Content-Type: application/json
```

```json
{
  "challengeId": "00000000-0000-0000-0000-000000000000",
  "challengeToken": "<challenge-token>",
  "code": "123456"
}
```

Success returns `200 OK`, sets `__Host-veritas-admin`, and returns:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "email": "admin@example.com",
  "name": "First Admin"
}
```

## Authenticated Requests

After MFA succeeds, the browser sends `__Host-veritas-admin` automatically.

For unsafe methods, also send:

```http
X-CSRF-TOKEN: <csrf-token>
```

The server validates the cookie against the Users-owned admin session. A cookie can be rejected if:

- The server-side session is revoked.
- The idle or absolute session lifetime has expired.
- The admin security stamp no longer matches.
- The backing admin account no longer has MFA enabled.
- Data Protection keys cannot decrypt the cookie.

## Logout

```http
POST /api/v1/admin-auth/logout
X-CSRF-TOKEN: <csrf-token>
```

Logout revokes the current server-side admin session and clears the auth cookie.

## Postman Testing Checklist

1. Use HTTPS.
2. Call `GET /api/v1/admin-auth/csrf`.
3. Keep cookies enabled in Postman.
4. Add `X-CSRF-TOKEN` to unsafe requests.
5. Call `POST /api/v1/admin-auth/login`.
6. Complete either `mfa/enroll/confirm` or `mfa/verify`.
7. Confirm Postman stores `__Host-veritas-admin`.
8. Include the CSRF header for protected `POST`, `PUT`, and `DELETE` requests.

## Security Notes

- Never store the admin password, TOTP code, challenge token, recovery code, or TOTP secret in local storage.
- Do not log challenge tokens, TOTP secrets, TOTP codes, recovery codes, session ids, or cookie values.
- Recovery codes are one-time use.
- Password login failures and MFA failures intentionally return generic authentication errors.
