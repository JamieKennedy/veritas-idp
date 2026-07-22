# Public Repository Publication Checklist

This checklist is the owner runbook for changing the existing GitHub repository from private to public. Complete it against the exact `main` commit approved in the [security review](../security/review-summary.md).

## Freeze and disclosure audit

- [ ] Pause merges and record the candidate commit SHA.
- [ ] Create an encrypted offline `git bundle` containing every branch and tag and record its checksum outside the repository.
- [ ] Scan every reachable commit, branch, and tag; rotate any real credential before removing it from history.
- [ ] Review author metadata, commit messages, deleted files, issues, pull requests, comments, releases, Actions logs and artifacts, environments, deployment names, and package metadata as if already public.
- [ ] Delete stale remote branches only after confirming the offline bundle.
- [ ] Confirm all retained brand assets, code, documentation, and generated content have known compatible provenance.

## Candidate verification

- [ ] `pnpm check` passes with Node 22 and pnpm 10.15.1.
- [ ] `pnpm check:secrets` passes across the complete reachable history.
- [ ] `pnpm check:containers` and the Compose configuration checks pass.
- [ ] Dependency and image audits contain no unresolved High or Critical finding.
- [ ] The threat-focused manual review meets the severity policy in the review summary.
- [ ] A fresh clone completes bootstrap, MFA login, SMTP configure/defer, restart persistence, and a controlled dependency-failure exercise.
- [ ] The README, security policy, support policy, contribution guide, issue forms, and release notes consistently state pre-alpha status.

## Visibility change

- [ ] Merge the reviewed candidate to `main` through the normal protected promotion path.
- [ ] Confirm visible releases and container metadata do not imply production support.
- [ ] Change repository visibility to public and immediately review the documented [visibility consequences](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/managing-repository-settings/setting-repository-visibility).
- [ ] Re-enable or verify the `main` and `staging` rulesets, every required check, force-push protection, deletion protection, conversation resolution, and merge-method settings.
- [ ] Verify Actions use read-only default permissions and first-time fork pull requests require approval.
- [ ] Enable private vulnerability reporting, the dependency graph, Dependabot alerts and security updates, secret scanning, push protection, and CodeQL for C# and JavaScript/TypeScript.

## Post-publication verification

- [ ] Review the first GitHub secret-scanning, Dependabot, and CodeQL results before announcing the repository.
- [ ] Confirm the private vulnerability-reporting button and security issue redirect work.
- [ ] Confirm the GitHub community profile recognizes the license and community documents.
- [ ] Open a harmless pull request from a fork and verify it cannot access secrets or release permissions.
- [ ] Recheck rulesets and required status checks after the first public pull request.
- [ ] Schedule an independent assessment before removing the experimental warning or recommending production use.
