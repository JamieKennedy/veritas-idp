# Veritas Threat Model

## Status and scope

This threat model covers the pre-alpha Admin UI and Admin API, bootstrap and administrator authentication, PostgreSQL persistence, RabbitMQ messaging, SMTP configuration, the database migrator, Aspire development orchestration, and Docker Compose deployment.

OAuth 2.0, OpenID Connect, public-user authentication, federation, client registration, signing keys, and public APIs are not implemented and are outside this model. Adding any of them requires updating this document before the capability is released.

## Security objectives

Veritas must:

- prevent unauthenticated creation of an administrator after bootstrap;
- prevent reuse or disclosure of bootstrap, login-challenge, session, TOTP, recovery-code, and SMTP secrets;
- avoid revealing whether credentials, accounts, challenges, or sessions are valid;
- preserve exactly-once security outcomes under duplicate and concurrent requests;
- protect state-changing browser requests against cross-site request forgery;
- keep module-owned data isolated even though modules share a PostgreSQL database;
- fail closed when required persistence, messaging, or secret configuration is unavailable;
- leave operational evidence without writing secrets or personal data to logs.

Availability against a privileged host administrator, database administrator, or denial-of-service actor with infrastructure access is not currently guaranteed.

## Assets

| Asset                                  | Required property                                                                            |
| -------------------------------------- | -------------------------------------------------------------------------------------------- |
| Bootstrap deployment secret            | High entropy, deployment-controlled, never persisted or logged in plaintext                  |
| Bootstrap session token                | Short lived, stored client-side only in a secure cookie, hashed server-side                  |
| Administrator password                 | Hashed with the platform password hasher; never logged or returned                           |
| TOTP secret and recovery codes         | Protected at rest, revealed only during enrollment or generation, one-time recovery-code use |
| Login challenge and admin session      | Short-lived/revocable, hashed or represented by non-secret identifiers as appropriate        |
| SMTP secret                            | Protected at rest and never returned by read APIs                                            |
| Data-protection keys                   | Persistent and access-restricted; compromise permits decryption or cookie forgery            |
| PostgreSQL data                        | Private network access, least-privilege credentials, module-owned schemas                    |
| RabbitMQ credentials and messages      | Private network access and payloads free of raw secrets                                      |
| Logs, traces, errors, and build output | Useful operational context without credentials, tokens, hashes, or sensitive content         |

## Trust boundaries and data flow

1. A browser crosses the public ingress boundary through an operator-managed HTTPS reverse proxy to the Admin UI and Admin API.
2. The Admin UI is untrusted input. The API validates request shape, CSRF tokens, authentication cookies, authorization, and rate limits.
3. The Admin API composes Platform, Users, and Messaging modules in process. Application ports define cross-module access; one module must not query another module's DbContext.
4. Module infrastructure crosses the persistence boundary to PostgreSQL. Database access does not make data safe to expose through another module or transport.
5. Wolverine crosses the messaging boundary to RabbitMQ. Durable message contracts cannot contain raw secrets or rendered sensitive content.
6. SMTP delivery crosses an external-provider boundary. Connection failure and remote responses are untrusted and must not leak credentials into errors or logs.
7. The migrator has elevated schema privileges and runs before the API. Its advisory lock, retries, and cancellation behavior protect startup consistency.

## Principal threats and controls

| Threat                                          | Current controls                                                                                              | Required verification                                            |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| Guessing or replaying bootstrap requests        | Deployment secret, fixed-time comparison, short-lived hashed session token, unique active slot, rate limiting | Invalid, expired, replayed, and concurrent request tests         |
| Creating multiple initial administrators        | Unique initial-admin slot and application checks                                                              | Real PostgreSQL concurrency test proving one winner              |
| Credential stuffing and account enumeration     | Generic failures, rate limiting, password hashing, mandatory MFA                                              | HTTP tests for identical external errors and rate-limit behavior |
| Replaying MFA challenges or recovery codes      | Hashed challenge tokens, expiry/state checks, one-time recovery-code consumption                              | Expiry, replay, and concurrent-consumption tests                 |
| Session theft or continued use after revocation | Secure, HTTP-only, strict SameSite cookie; server-side session validation and revocation                      | Cookie-policy and revoked-session tests                          |
| Cross-site state changes                        | Global antiforgery validation, strict SameSite cookies, explicit credentialed CORS origins                    | Host-level CSRF and hostile-origin tests                         |
| Secret disclosure through APIs or diagnostics   | Safe DTOs, structured logging rules, protected SMTP/TOTP storage                                              | Response, logging, exception, and scanner review                 |
| Migration races or partial startup              | PostgreSQL advisory lock, bounded retry, cancellation propagation, API wait ordering                          | Concurrent migrator and dependency-failure tests                 |
| Compromised dependency or build workflow        | Lockfiles, pinned Actions, dependency audits, container scans, read-only PR permissions                       | CI review and alert triage                                       |
| Committed credential                            | Ignore rules, full-history Gitleaks job, GitHub secret scanning and push protection after publication         | Zero unresolved findings before publication                      |

## Deployment assumptions

- Only the reverse proxy is exposed publicly, and it terminates trusted HTTPS.
- PostgreSQL, RabbitMQ, and internal application ports are not internet-accessible.
- Operators generate independent high-entropy database, messaging, and bootstrap secrets.
- Operators persist and restrict data-protection keys; ephemeral keys invalidate protected data and sessions on restart.
- Backups, host hardening, firewall rules, certificate management, monitoring, and incident response remain operator responsibilities.
- Development defaults and example credentials are never reused in an internet-facing environment.

## Known limitations

- Veritas has not received an independent security assessment.
- Pre-alpha releases do not have compatibility or security-support guarantees.
- The current review does not establish resistance to a malicious host administrator or database administrator.
- Rate limiting is process-local and is not a complete distributed abuse-prevention system.
- Audit storage and alerting are incomplete.
- Production-scale high availability, backup restoration, key rotation, and disaster recovery have not been validated.

These limitations block a production-readiness claim but do not relax the requirement to fix known exploitable defects in the implemented surface.

## Review triggers

Update this model when a change adds an externally reachable endpoint, credential or token type, trust boundary, privileged dependency, persistent secret, authentication method, deployment topology, or cross-module data flow.
