# Development database reset design

## Purpose

Provide a development-only PowerShell script that returns Veritas's local PostgreSQL database to the same empty state as a fresh installation. The next Aspire startup will recreate the database and apply the existing migrations.

## Scope

The script targets only Veritas PostgreSQL data volumes. It does not modify Redis, RabbitMQ, logs, data-protection keys, secrets, source files, or Docker resources belonging to other projects.

## Design

`scripts/reset-dev-database.ps1` will:

1. Require an explicit `-Confirm` switch to perform destructive work.
2. Verify that Docker is available.
3. Find only the known local PostgreSQL volume names used by Aspire and Docker Compose (`veritas-postgres-data` and `veritas_veritas-postgres-data`).
4. Refuse to proceed when a container is currently using a matching volume, instructing the developer to stop `pnpm dev` or Docker Compose first.
5. Remove each matching volume and report what it removed. If no matching volume exists, it will report that the database is already absent.
6. Print the next step: run `pnpm dev`, which recreates the database and runs migrations.

## Error handling

The script stops on errors, provides a clear message when Docker is unavailable or a target volume is attached, and never falls back to wildcard or broad volume deletion. Removing one target volume does not cause it to delete a different volume.

## Verification

Pester tests will cover volume discovery, confirmation guarding, attached-volume refusal, no-volume behavior, and removal of only recognized names. The script will also be checked with PowerShell's parser.
