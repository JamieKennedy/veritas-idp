# Branching and releases

Veritas uses one integration branch and one production branch:

```text
feature/* | bugfix/* | chore/* | docs/* -> staging -> main
```

`staging` is the normal PR target and the source of prereleases. `main` contains only production releases. There is no separate long-lived `dev` branch because it would duplicate the integration state already represented by `staging`.

## Branch names

Use a short lowercase description:

- `feature/<name>` for new behavior.
- `bugfix/<name>` for a non-production defect.
- `chore/<name>` for maintenance, dependencies, or tooling.
- `docs/<name>` for documentation-only work.
- `hotfix/<name>` for an urgent fix branched from production.

Dependabot-managed branches use its standard `dependabot/*` prefix.

## Pull requests

Normal work starts from `staging`, targets `staging`, and is squash merged. The PR title becomes the squash commit and must use Conventional Commits:

```text
feat(auth): add passkey enrollment
fix(messaging): retry a timed-out SMTP delivery
chore(ci): update the container scanner
```

Supported release types are:

- `feat` produces a minor release.
- `fix` and `perf` produce a patch release.
- `!` or a `BREAKING CHANGE` footer produces a major release.
- `build`, `chore`, `ci`, `docs`, `refactor`, and `test` do not release by themselves.

All required checks must pass and all conversations must be resolved. Do not push directly to `staging` or `main`.

## Prereleases

Merging to `staging` does not automatically build or publish containers. This keeps normal integration work limited to the five required PR gates and allows several changes to be batched into one prerelease.

When the desired changes are present on `staging`, start a prerelease manually:

1. Open **Actions → Pipeline → Run workflow**.
2. Select the `staging` branch.
3. Check **Confirm that this staging prerelease should be built and published**.
4. Run the workflow.
5. Confirm all five gates pass before the release job starts.

The workflow records the exact `staging` commit selected at dispatch and reruns the source and container gates against that commit. It rejects another branch or an unconfirmed request before starting the expensive jobs. Semantic Release evaluates all commits since the preceding release and, when at least one qualifies, creates:

- a `vX.Y.Z-beta.N` Git tag;
- a GitHub prerelease;
- immutable `X.Y.Z-beta.N` and `sha-<commit>` container tags;
- the moving `beta` container tag;
- a versioned Docker Compose bundle and checksum.

Commits that contain only non-releasing Conventional Commit types pass the manually requested validation but do not create an empty prerelease. Do not dispatch a prerelease solely to validate ordinary staging work; the required PR gates already provide that validation.

Before production promotion, deploy and test the exact immutable prerelease with the procedure in [Docker Compose deployment](../operations/docker-compose.md). Record that version in the promotion PR.

## Production promotion

Open a PR from `staging` to `main`. Complete the production-promotion section of the PR template with the exact beta version tested. Use a merge commit rather than squash or rebase so Semantic Release can see the commits already exercised in staging.

After the protected checks pass and the PR is merged, the `main` push reruns every gate and Semantic Release automatically creates:

- the stable `vX.Y.Z` tag and GitHub Release;
- immutable `X.Y.Z` and `sha-<commit>` container tags;
- the moving `latest` container tag;
- stable release notes and a versioned Compose bundle.

Git tags are the canonical version source. Private JavaScript manifests use `0.0.0-development`; build-time version metadata is injected into assemblies, frontend output, and OCI image labels.

## Production hotfixes

1. Branch `hotfix/<name>` from `main`.
2. Add the smallest safe fix and its regression test.
3. Open a PR directly to `main` with a `fix:` or breaking Conventional Commit title.
4. Run all required checks and merge by squash.
5. Verify the production release.
6. Immediately open a `main -> staging` PR and merge it with a merge commit.

Never fix only `main`; the propagation PR prevents the defect from reappearing in the next normal release.

## Merge settings

The repository permits squash merges and merge commits. Rebase merging is disabled:

- topic PRs into `staging`: squash;
- `staging -> main`: merge commit;
- `hotfix/* -> main`: squash;
- `main -> staging` propagation: merge commit.

Linear-history enforcement is intentionally disabled because promotion and propagation retain ancestry through merge commits.
