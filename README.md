# Veritas

Veritas is an experimental, self-hostable authentication service and identity provider under active development. It uses a .NET 10 modular monolith, a React administration UI, PostgreSQL, RabbitMQ, .NET Aspire for local orchestration, and Docker Compose for deployment.

> [!WARNING]
> Veritas is pre-alpha software. It is incomplete, receives breaking changes without migration guarantees, and must not be used to protect production systems or real user identities.

## Current capabilities

The current source tree implements:

- first-run bootstrap guarded by a deployment secret;
- an initial administrator with password hashing;
- mandatory TOTP enrollment, recovery codes, and server-side admin sessions;
- CSRF protection, restricted credentialed CORS, and rate limiting on sensitive endpoints;
- SMTP configuration and templated email delivery;
- module-owned PostgreSQL schemas and migrations;
- local Aspire orchestration and a Docker Compose deployment bundle.

OAuth 2.0, OpenID Connect, public-user authentication, client management, signing-key management, federation, comprehensive audit storage, and production upgrade guarantees are not implemented yet.

## Development

Install .NET 10, Node.js 22, pnpm 10.15.1, and Docker. From the repository root:

```powershell
pnpm restore
pnpm dev
```

The Aspire AppHost requires a locally configured `bootstrap-secret`. See [bootstrap and first-admin setup](docs/bootstrap.md) for the development and deployment options.

Before submitting a change, run:

```powershell
pnpm check
```

## Documentation

- [Documentation index](docs/README.md)
- [Bootstrap and first-admin setup](docs/bootstrap.md)
- [Admin authentication flow](docs/admin-auth-flow.md)
- [Docker Compose deployment](docs/operations/docker-compose.md)
- [Security model and threat boundaries](docs/security/threat-model.md)
- [Coding standards](docs/engineering/coding-standards.md)
- [Branching and releases](docs/engineering/branching-and-releases.md)

## Project policy

- Security vulnerabilities: [security policy](SECURITY.md)
- Contributions: [contribution guide](CONTRIBUTING.md)
- Support expectations: [support policy](SUPPORT.md)
- Community conduct: [code of conduct](CODE_OF_CONDUCT.md)

Development is maintainer-led. APIs, configuration, schemas, and deployment behavior may change before 1.0. Discuss substantial work before implementation so it can be aligned with the architecture and roadmap.

## License

Veritas is licensed under the [GNU Affero General Public License v3.0 only](LICENSE).
