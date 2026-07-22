# Contributing to Veritas

Veritas welcomes focused contributions, but development is maintainer-led while the architecture and security model are still evolving.

## Before implementation

Open an issue before starting substantial features, schema changes, new dependencies, module-boundary changes, or security-sensitive work. Small documentation corrections and obvious bug fixes may be submitted directly.

Security vulnerabilities must be reported privately through the process in [SECURITY.md](SECURITY.md).

## Development workflow

1. Branch from `staging` using the naming rules in the [branching guide](docs/engineering/branching-and-releases.md).
2. Keep the change focused and follow the repository and scoped `AGENTS.md` guidance.
3. Add tests for meaningful behavior, including relevant failure, cancellation, retry, replay, and concurrency cases.
4. Run `pnpm fix`, review the result, and run `pnpm check`.
5. Open a pull request to `staging` using a Conventional Commit title.

Pull requests from forks run with read-only permissions and do not receive release credentials. Maintainers may close work that was not discussed and conflicts with the roadmap or security posture.

## Contribution license

No contributor license agreement is currently required. By submitting a contribution, you certify that you have the right to provide it and agree that it is licensed under AGPL-3.0-only with the rest of the project.

Do not submit proprietary code, confidential information, real credentials, personal data, or material whose license is incompatible with AGPL-3.0-only.
