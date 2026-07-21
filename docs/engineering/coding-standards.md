# Veritas Coding Standards

This handbook is the canonical reference for how Veritas code is written. It applies to humans and AI agents. Repository and scoped `AGENTS.md` files remain authoritative for architecture, module ownership, security, persistence, messaging, and operational requirements.

Mechanical rules belong in `.editorconfig`, `Directory.Build.props`, Prettier, ESLint, and TypeScript configuration. This handbook explains the intent behind them and defines standards that require engineering judgment. If prose and executable configuration disagree on a mechanical rule, treat the failing check as authoritative and correct both sources together.

## Required workflow

- Run `pnpm fix` after editing to apply safe C# and frontend formatting and lint fixes.
- Run `pnpm check` before handoff. It must complete without warnings or errors.
- Run `pnpm check:containers` after changing a Dockerfile, Compose, container runtime behavior, or release packaging.
- Never edit generated files. Regenerate them through their owning tool.
- Add or update tests for meaningful behavior changes and bug fixes.
- Keep suppressions narrow, local, and justified as described below.

## Pull request gates

Every pull request to `staging` or `main` must pass these stable GitHub checks:

| Check              | Enforced requirements                                                                                                                 |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------- |
| `PR policy`        | Branch prefix, Conventional Commit PR title, permitted production source, promotion evidence, and manual release authorization        |
| `Repository lint`  | Project registration, generated routes, release-image alignment, Prettier, Markdownlint, Actionlint, Hadolint, and Compose validation |
| `Backend quality`  | Restore, C# formatting and style, analyzers, warnings-as-errors build, tests, and NuGet audit                                         |
| `Frontend quality` | Frozen install, Prettier, ESLint without warnings, strict TypeScript, build, tests, and pnpm audit                                    |
| `Container build`  | Shared backend and Admin UI image builds, entry-point verification, and high/critical vulnerability scanning                          |

Mechanical requirements belong in repository configuration and scripts so local and CI behavior stay aligned. Architecture, threat modeling, appropriate test scope, operational safety, and documentation accuracy still require reviewer judgment and are recorded in the PR checklist.

The canonical commands are:

```powershell
pnpm fix
pnpm check
pnpm check:containers
pnpm lint:workflows
```

`pnpm check:containers` requires a running Docker daemon. The complete production-shaped stack remains a manual staging acceptance procedure documented in [`../operations/docker-compose.md`](../operations/docker-compose.md).

## Project registration

Quality checks cover projects by convention:

- Every tracked `.csproj` under `src` must be registered in `Veritas.slnx`. Root .NET formatting, analyzers, build, test, and audit commands then cover it automatically.
- Every frontend application must be a private package under `src/frontend`, be matched by `pnpm-workspace.yaml`, and define `build`, `format:check`, `lint`, `test`, and `typecheck` scripts.
- `pnpm-lock.yaml` is the only JavaScript lockfile. Do not introduce npm or Yarn lockfiles.
- New Dockerfiles must be added to Hadolint and container-build coverage.
- New API or UI images must be added to the release script, Compose deployment, image scanner, and operational documentation in the same PR.

## Shared engineering standards

- Optimize for clear contracts, cohesive units, and explicit data flow. A type or module should have one reason to change.
- Make invalid states difficult to represent. Validate untrusted data at the boundary where it enters the system.
- Prefer immutable values and pure transformations. Mutation should be deliberate, local, and easy to observe.
- Use guard clauses for invalid input and failed preconditions when they keep the happy path clear.
- Avoid magic values, boolean-parameter traps, hidden global state, premature abstractions, and speculative extension points.
- Name code after domain intent. Do not use vague names such as `Helper`, `Manager`, `Data`, or `Utils` when a precise capability name exists.
- Comments explain intent, constraints, security reasoning, compatibility requirements, or non-obvious trade-offs. They do not narrate syntax.
- Keep secrets, credentials, hashes, tokens, personal data, and internal topology out of logs, exceptions, test output, and comments.

## C# and .NET

### Structure and language use

- Use file-scoped namespaces and one primary type per file. Small private nested types may remain with their owner.
- Use braces for every control-flow body.
- Prefer `var` for local variables in all cases. An explicit local type is acceptable only when inference materially obscures a non-obvious type; suppress `IDE0007` at the smallest scope and explain why.
- Prefix interfaces with `I`. Use PascalCase for types, methods, properties, events, constants, and enum members. Use `_camelCase` for private instance fields. Do not prefix enums with `E`.
- Suffix asynchronous methods with `Async`. Do not use `async void` except for framework-required event handlers.
- Seal concrete classes by default when inheritance is not part of their contract. Do not add inheritance solely for testing.
- Use records for immutable data carriers and classes for entities with identity or intentional mutable lifecycle.
- Prefer framework and language features over custom equivalents when the framework contract is clear.
- Keep nullable annotations truthful. Do not use the null-forgiving operator to silence an unresolved flow problem.

### APIs, failures, and dependencies

- Keep public and cross-module interfaces small. Accept the least-powerful abstraction that expresses the required capability.
- Use `FluentResults` for expected Domain/Application failures. Use exceptions for unexpected failures and dependency faults that cannot be represented locally.
- Propagate `CancellationToken` through asynchronous I/O and dependency calls. Do not replace caller cancellation with `CancellationToken.None`.
- Use `TimeProvider` for behavior that depends on the current time. Persist and communicate UTC timestamps with a `Utc` suffix.
- Use structured logging with named properties. Log once at the boundary that has enough context to act, without exposing sensitive values.
- Application use cases own transaction timing and call `SaveChangesAsync` once per atomic operation.
- Dispose owned resources deterministically. Do not dispose injected dependencies unless ownership is explicitly transferred.

### XML documentation

- Document public contracts, interfaces, externally consumed DTOs, and non-obvious internal behavior.
- Use `<inheritdoc />` for straightforward interface or base implementations.
- Describe parameters, return values, formats, nullability meanings, security properties, and known exceptions where they form part of the contract.
- Private helpers need XML documentation only when naming and structure cannot express an important constraint.
- Keep documentation synchronized with behavior. Stale documentation is a defect.

## TypeScript and React

### Formatting and types

- Prettier owns formatting: four-space indentation, 160-column print width, single quotes, trailing commas, and no semicolons.
- Keep TypeScript strict. Do not use `any` without a narrow, justified ESLint suppression. Prefer `unknown` followed by validation or narrowing.
- Validate external HTTP, environment, storage, and message data at runtime. Infer types from Zod schemas when the schema is the source of truth.
- Use interfaces for extendable object contracts and declaration merging. Use type aliases for unions, mapped types, and schema-inferred types.
- Use named exports. Default exports are limited to framework or tooling requirements.

### Components and state

- Use function components and call hooks unconditionally at the component top level.
- Keep feature code colocated by capability. Shared code must have multiple genuine consumers and a stable purpose before moving into a shared folder.
- TanStack Query owns server state. Do not duplicate query data into component state.
- Use effects to synchronize with external systems, not to derive values that can be computed during render.
- Use reducers for multi-step workflows and state machines whose transitions deserve explicit names.
- Keep props focused. Extract a named props contract when an inline type becomes difficult to scan or is shared.
- Use semantic HTML first. Interactive behavior must support keyboard input, visible focus, programmatic labels, and accessible error/status announcements.

## Testing

- Test observable behavior, contracts, security-sensitive transitions, and meaningful failure paths rather than private implementation details.
- Add a regression test before fixing a bug and confirm that it fails for the expected reason.
- Keep tests deterministic by injecting time, randomness, and external boundaries where behavior depends on them.
- Prefer real domain/application objects. Mock only boundaries that are unavailable, slow, nondeterministic, or outside the test's scope.
- Use Arrange-Act-Assert when it improves readability; do not add ceremonial comments to self-evident tests.
- There is no numeric coverage target. Untested meaningful behavior is still incomplete even when an aggregate percentage is high.

## Generated code

Generated files are excluded centrally from formatting and analyzers when the owning generator cannot produce compliant output. Current generated areas include EF Core migrations/designers and TanStack's `routeTree.gen.ts`.

- Never hand-edit generated output to satisfy a style check.
- Change the generator configuration or source model, regenerate, and review the resulting semantic diff.
- Generated exclusions must name a known path or pattern. Do not classify handwritten code as generated.

## Suppressions and exceptions

A suppression is a documented engineering decision, not a shortcut.

1. Confirm the diagnostic is understood and the code cannot reasonably satisfy it without harming correctness, clarity, compatibility, or an intentional boundary.
2. Suppress only the specific rule at the smallest useful scope.
3. Place a concise reason beside the suppression. State the constraint, not merely that the warning is unwanted.
4. Prefer a local suppression. Central exceptions are reserved for a repeated repository-wide conflict and must be explained in `.editorconfig`.
5. Never suppress compiler errors, nullable-flow errors, dependency vulnerability warnings, or security diagnostics merely to make CI pass.

Approved central exceptions are `CA1848` (source-generated logging is reserved for measured hot paths), `CA1859` (concrete collection types conflict with intentional abstractions), test-scoped `CA1707` and the async-suffix naming rule (behavior-style test names use underscores for readable runner output), and scoped `CA1716` for the established `Veritas.Shared` namespace. TOTP's protocol-required HMAC-SHA1 use has a local `CA5350` suppression with an interoperability explanation.
