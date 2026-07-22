# Security Policy

Veritas is pre-alpha authentication infrastructure. No release is currently supported for production use, and the project does not provide a security-response service-level agreement.

## Reporting a vulnerability

Do not open a public issue, pull request, discussion, or comment for a suspected vulnerability.

Use the repository's **Security → Advisories → Report a vulnerability** flow. This creates a private report visible to the maintainer. Include:

- the affected version or commit;
- prerequisites and a minimal reproduction;
- the expected and observed security impact;
- suggested mitigations, if known;
- whether the issue has been disclosed elsewhere.

Reports are handled on a best-effort basis. The maintainer will acknowledge the report when practical, validate its scope, coordinate a fix, and agree on disclosure timing with the reporter. Please allow a reasonable remediation window before public disclosure.

## Supported versions

There are no production-supported versions. Security fixes are applied to the active development line and included in the next release. Older prereleases are not maintained.

## Security boundaries

The current trust model and known limitations are documented in the [Veritas threat model](docs/security/threat-model.md). A clean automated scan is not proof that the system is secure.
