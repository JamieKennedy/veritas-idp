# Veritas-IDP 🛡️

**Veritas-IDP** is a high-performance, self-hosted Identity Provider (IDP) designed to be the single "Source of Truth" for your applications. Built on a strict **Onion Architecture**, it balances robust security (OpenBao, OpenFGA) with modern developer experience (.NET 10, .NET Aspire, Wolverine).

## Documentation

- Project docs index: [`docs/README.md`](docs/README.md)
- Setup token guide: [`docs/setup-token.md`](docs/setup-token.md)
- Coding standards: [`docs/engineering/coding-standards.md`](docs/engineering/coding-standards.md)
- Branching and releases: [`docs/engineering/branching-and-releases.md`](docs/engineering/branching-and-releases.md)
- Docker Compose deployment: [`docs/operations/docker-compose.md`](docs/operations/docker-compose.md)

## Reset the development database

Stop the local stack, then remove only Veritas's PostgreSQL development volume:

```powershell
./scripts/reset-dev-database.ps1 -Confirm
```

Start `pnpm dev` afterwards. Aspire recreates the database and applies migrations. This command does not clear Redis, RabbitMQ, logs, or data-protection keys.
