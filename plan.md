## Core principles

- Backend: **.NET**
- Frontend: **React**
- Messaging/events: **RabbitMQ**
- Cache: **Redis**
- Database: **Postgres, one DB per service**
- Service structure: **Onion architecture**
- Cross-service communication:
    - **Sync** for immediate validation/query needs
    - **Async events** for side effects, auditing, notifications, webhooks
- Development orchestration: **.NET Aspire**

---

# Service breakdown

## 1. Auth Service

**Purpose:** Core identity and protocol engine.

**Owns:**

- Login/authentication orchestration
- OAuth2 / OIDC flows
- Token issuance
- Authorization codes
- Refresh tokens
- Revocation/introspection
- Consent orchestration
- Logout orchestration

**Depends on:**

- User Service
- Client/Project Service
- Session Service
- Key Management Service
- Platform Service

**Storage examples:**

- Authorization codes
- Refresh token families
- Login transactions
- Consent grants

**Notes:**

- Most security-sensitive service
- Use Redis for short-lived auth flow state

---

## 2. User Service

**Purpose:** User identities and credentials.

**Owns:**

- Users
- Emails/usernames/phones
- Password hashes
- MFA enrollment
- Recovery codes
- Verification state
- Account status (locked, disabled, archived)

**Storage examples:**

- Users
- Credentials
- MFA factors
- Recovery codes
- Verification records

**Publishes events like:**

- `UserCreated`
- `PasswordChanged`
- `UserLocked`
- `MfaEnabled`

**Notes:**

- Never expose password hashes or secrets
- Keep profile/credential ownership here

---

## 3. Client / Project Service

**Purpose:** Tenant/project/client application configuration.

**Owns:**

- Projects/tenants
- OAuth clients
- Redirect URIs
- Client secrets
- Allowed grant types
- Allowed scopes
- Client policies
- Branding settings

**Storage examples:**

- Projects
- Clients
- RedirectUris
- ClientSecrets
- AllowedScopes

**Publishes events like:**

- `ProjectCreated`
- `ClientCreated`
- `ClientSecretRotated`
- `ClientDisabled`

**Notes:**

- Cache hot client metadata in Redis
- This is the main config source for apps/clients

---

## 4. Session Service

**Purpose:** Session lifecycle and SSO state.

**Owns:**

- Browser sessions
- SSO sessions
- Remember-me sessions
- Session revocation
- Timeout tracking
- Session activity

**Storage examples:**

- Sessions
- SessionDevices
- SessionRevocations
- SessionActivity

**Publishes events like:**

- `SessionCreated`
- `SessionRevoked`
- `UserLoggedOut`

**Notes:**

- Redis is useful for active session lookup
- Keep session state separate from token state

---

## 5. Key Management Service

**Purpose:** Signing/encryption key lifecycle.

**Owns:**

- Signing keys
- Rotation policies
- Active/retired key state
- Public key metadata / JWKS projection

**Storage examples:**

- Key metadata
- Key versions
- Rotation policies

**Publishes events like:**

- `SigningKeyRotated`
- `SigningKeyRetired`
- `JwksChanged`

**Notes:**

- Best integrated with Vault for private key storage
- Postgres should mainly store metadata, not raw private keys

---

## 6. Notification Service

**Purpose:** Message delivery.

**Owns:**

- Email sending
- SMS sending
- Templates
- Delivery retries
- Delivery status tracking

**Storage examples:**

- NotificationMessages
- Templates
- DeliveryAttempts
- ProviderResponses

**Consumes events like:**

- `SendEmailRequested`
- `SendSmsRequested`

**Publishes events like:**

- `NotificationSent`
- `NotificationFailed`

**Notes:**

- Mostly async/event-driven
- Use inbox/outbox and idempotent consumers

---

## 7. Audit Service

**Purpose:** Immutable audit trail.

**Owns:**

- Security audit logs
- Admin action logs
- Export/search of audit history

**Storage examples:**

- AuditEvents
- AuditActors
- AuditTargets
- CorrelationRecords

**Consumes events from:**

- All services

**Notes:**

- Append-only design
- Separate from normal app logging

---

## 8. Webhooks Service

**Purpose:** External event delivery.

**Owns:**

- Webhook endpoints
- Subscriptions
- Webhook secrets
- Retry logic
- Delivery history
- Replay/dead-letter handling

**Storage examples:**

- WebhookEndpoints
- WebhookSubscriptions
- DeliveryAttempts
- WebhookSecrets

**Consumes events from:**

- Relevant domain services

**Notes:**

- Sign outgoing payloads
- Disable failing endpoints after threshold
- Use retries and dead-letter handling

---

## 9. Platform Service

**Purpose:** Global platform/system state.

**Owns:**

- Setup/bootstrap state
- System settings
- Feature flags
- Instance metadata
- Global booleans/config

**Storage examples:**

- SystemSettings
- FeatureFlags
- SetupState
- InstanceMetadata

**Publishes events like:**

- `SetupCompleted`
- `SystemSettingChanged`
- `FeatureFlagChanged`

**Notes:**

- This is where `SETUP_COMPLETE` belongs
- Prefer a `SystemSetting` model over only boolean flags
- Better to model setup as a small state machine, not a single bool

---

## 10. Admin UI

**Purpose:** Admin/operator frontend.

**Features:**

- User management
- Client/project management
- Audit viewing
- Webhook management
- Platform settings
- Setup flow
- Key rotation/admin actions
- Session management

**Notes:**

- React app only
- Should call backend APIs, not contain business logic
- Prefer routing/features by domain

---

## 11. Public UI

**Purpose:** End-user frontend.

**Features:**

- Login
- Registration
- Consent
- Password reset
- MFA setup/challenge
- Account management
- Session/device management
- Email verification
- Logout

**Notes:**

- Security-sensitive React app
- Mostly interacts with Auth, User, and Session APIs
- Should support project/client branding if needed

---

# Platform-wide implementation rules

## Architecture inside each backend service

Each service should follow onion architecture with:

- **Domain**
    - Entities
    - Value objects
    - Business rules
    - Domain events
- **Application**
    - Commands/queries
    - Handlers
    - Use cases
    - Service interfaces
- **Infrastructure**
    - EF Core
    - RabbitMQ integration
    - Redis integration
    - External providers/clients
    - Outbox/inbox implementation
- **API**
    - Controllers/endpoints
    - Validation wiring
    - Auth middleware
    - OpenAPI
    - Health checks

---

# Communication rules

## Use synchronous calls for:

- Immediate validation
- Security checks
- User-facing flows needing instant response

Examples:

- Auth validating a client
- Auth checking user credentials
- Auth checking setup state

## Use async events for:

- Notifications
- Audit logging
- Webhook delivery
- Cross-service side effects
- Projections/reactions

Examples:

- `UserCreated`
- `PasswordChanged`
- `SessionRevoked`
- `SetupCompleted`

---

# Data and messaging patterns

## Database

- One **Postgres DB per service**
- No direct cross-service DB access
- Each service owns its schema and write model

## Messaging

- RabbitMQ for integration events
- Use **versioned events**
- Consumers must be **idempotent**
- Include:
    - event id
    - correlation id
    - causation id
    - occurred at
    - actor id if relevant
    - tenant/project id if relevant

## Outbox pattern

Every service should use an **outbox table**:

1. Save DB changes
2. Save integration event in same transaction
3. Commit
4. Background publisher sends to RabbitMQ

## Inbox pattern

Consumers should track handled messages to avoid duplicate processing.

---

# Redis usage

Use Redis for:

- Auth flow state
- Session cache
- Rate limiting
- Temporary verification state
- Nonces/replay protection
- Hot client metadata
- Global settings cache
- Deduplication/short-lived locks

Do **not** use Redis as the primary source of truth.

---

# Cross-cutting concerns

Every backend service should include:

- Correlation IDs
- Structured logging
- Health checks
- Validation
- Authorization
- Idempotency
- Observability/metrics
- Secret management
- Audit event publishing where needed
- API/event versioning
