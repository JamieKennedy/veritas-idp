# GitHub repository settings

Repository configuration is part of the release control. Review these settings whenever required check names or collaborators change.

## General pull request settings

Under **Settings → General → Pull Requests**:

- enable squash merging;
- enable merge commits;
- disable rebase merging;
- enable automatic deletion of head branches;
- set the default squash commit message to the pull request title.

Keep `main` as the default branch. Keep write or maintain access limited to JamieKennedy while Veritas is a sole-maintainer repository.

## Staging ruleset

Create an active branch ruleset named `staging` targeting the branch `staging`.

- Do not grant a routine bypass actor.
- Restrict deletions.
- Block force pushes.
- Require a pull request before merging.
- Set required approvals to `0`; GitHub does not permit an author to approve their own PR.
- Require conversation resolution.
- Require status checks to pass.
- Require the branch to be up to date before merging.
- Do not require linear history.

Required checks:

- `PR policy`
- `Repository lint`
- `Backend quality`
- `Frontend quality`
- `Container build`

## Main ruleset

Create an active branch ruleset named `main` targeting the branch `main` with the same protections and required checks as `staging`.

The `PR policy` check additionally rejects any `main` PR unless its source is:

- `staging`, with an exact tested beta version and completed manual Compose test; or
- `hotfix/*`.

## Permissions and approvals

Rulesets control how a branch changes, but a personal repository cannot require the sole PR author to self-approve. With JamieKennedy as the only writer, merge authority is already limited to the owner.

If another writer is added:

1. Add `CODEOWNERS` entries naming JamieKennedy for protected and release-sensitive paths.
2. Require one code-owner approval.
3. Require dismissal of stale approvals.
4. Prevent the most recent pusher from satisfying the approval.

Do not add a broad administrator bypass. Use a time-limited, audited exception only for repository recovery.

## Protected branches

The active long-lived branches are `staging` and `main`. Both rulesets are active and require the five checks listed above. The former `dev` integration branch is retired; normal pull requests target `staging`.
