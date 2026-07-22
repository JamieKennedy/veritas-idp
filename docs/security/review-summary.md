# Public Readiness Security Review

## Review target

- Review date: 2026-07-21
- Reviewers: Codex-assisted standards and specification reviews
- Branch: `chore/public-readiness`
- Fixed point: `staging` at `169215819b7951dd7fcd967360a8c7b3a9949541`
- Candidate revision: the current PR head commit reported by GitHub

The exact candidate SHA is recorded by the PR and its required checks because a commit cannot contain its own SHA. Every passing check below must be rerun against the final PR head before publication approval.

## Scope

- repository tree and all reachable Git history;
- bootstrap and initial-administrator creation;
- administrator password, MFA, recovery-code, and session flows;
- cookie, CSRF, CORS, rate-limit, and external-error behavior;
- SMTP secret persistence and messaging contracts;
- database migration locking, retry, and cancellation;
- GitHub Actions permissions, dependencies, container images, and release controls.

## Evidence collected

| Evidence                | Result      | Notes                                                                                                                                                                                                                                |
| ----------------------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `pnpm check`            | Pass        | Formatting, lint, typecheck, build, 75 backend tests, and 81 frontend/server tests passed. The local host used pinned pnpm 10.15.1 with Node 26 rather than the required Node 22, so CI must repeat this check with `.node-version`. |
| `pnpm check:secrets`    | Pass        | Gitleaks 8.30.1, pinned by image digest, scanned 54 reachable commits with no leaks. CI repeats the scan across the complete committed PR head.                                                                                      |
| `pnpm check:containers` | Pass        | Compose validation, Dockerfile lint, both production image builds, entry-point checks, and Trivy High/Critical scans passed with zero findings. Unfixed findings are not ignored.                                                    |
| NuGet audit             | Pass        | All projects and transitive dependencies reported no known vulnerability from the configured sources.                                                                                                                                |
| pnpm audit              | Pass        | No known vulnerability was reported at the High threshold.                                                                                                                                                                           |
| Workflow lint           | Pass        | Actionlint passed after pinning Node for the secret scan and the .NET SDK for C# CodeQL analysis.                                                                                                                                    |
| Metadata and links      | Pass        | Public metadata, license declarations, required community files, and local Markdown links are consistent.                                                                                                                            |
| Two-axis review         | Conditional | No unresolved Critical or High code-review finding remains. The production-provider and host-level evidence gaps below remain publication blockers.                                                                                  |

Raw reports can contain sensitive matches or environment details and are not committed. This summary records only safe conclusions.

## Remediation completed

- Caller cancellation now propagates through bootstrap and administrator application services instead of being converted into generic external failures.
- Login-challenge and recovery-code consumption use EF concurrency tokens; relational SQLite tests prove one-winner behavior and transaction rollback for the exercised flows.
- Replay, expiry, invalid recovery-code, idle-session, bootstrap retry, and bootstrap replay cases have regression coverage.
- Full-history secret scanning and CodeQL for C# and JavaScript/TypeScript are required workflows with pinned dependencies and conservative permissions.
- Container scanning now fails on every known High or Critical vulnerability, including findings without a published fix.
- Public maturity, contribution, support, security-reporting, threat-model, and owner publication guidance are documented and mechanically checked.

## Publication blockers

- Commit the candidate, rerun all evidence on that immutable revision with Node 22 and pnpm 10.15.1, and record the SHA here.
- Add real PostgreSQL concurrency acceptance for initial-administrator creation, MFA challenge consumption, and recovery-code consumption.
- Add host-level tests for generic authentication errors, rate limiting, cookie flags, CSRF rejection, and hostile CORS origins.
- Add concurrent-migrator, unavailable-dependency, cancellation, and partial-failure acceptance coverage.
- Complete a fresh-clone Compose exercise covering bootstrap, MFA login, SMTP configure or defer, restart persistence, and a controlled dependency failure without example secrets.
- Complete the owner-only history and disclosure audit, then configure and verify GitHub rulesets, private vulnerability reporting, secret scanning and push protection, Dependabot, and CodeQL results.
- Obtain an independent assessment before removing the experimental warning or recommending production use.

## Decision

**Not approved for a public visibility change yet.** The repository-level safeguards and documentation are implemented and the available automated checks pass, but the immutable-candidate, production-PostgreSQL, host-boundary, migrator-failure, clean-clone, and owner-controlled gates above remain mandatory.
