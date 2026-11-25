---
title: Development guidelines
description: Shared engineering defaults supplied for the Veritas adoption review.
section: Development
sectionOrder: 4
order: 2
draft: true
---

Use this guide when starting an application or upgrading an existing one. It records the shared defaults for architecture, code style, integration, hosting and agent-assisted work. For an upgrade, identify what the change touches and adopt the relevant standards in stages. Record a deliberate departure in a short repository decision record with its scope, reason and any review or migration trigger.

Keep the canonical copy at the root of the organisation’s private `engineering-standards` repository. Application `AGENTS.md` files should link to the guide on the default branch. Use a commit permalink in an upgrade decision when the exact edition matters.

## Ownership and decision records

Assign an engineering owner or team to maintain the guide and review changes through PRs. Keep a changelog with the affected sections and adoption instructions. For each substantive change, state whether it applies to new apps, the next relevant upgrade or existing apps by a stated date.

Record application-specific architecture decisions and departures under `Docs/Adr/NNNN-short-title.md`. Include the context, decision, consequences, affected scope, owner and any review date or migration trigger.

## Start or upgrade an application

- Define the application’s audience, owner, boundaries and integrations. Choose the projects the application needs and add modules when there is a distinct area to own.
- Before upgrading an existing boundary, record the supported installation variants and starting versions, affected consumers and behaviour to preserve. Capture a repeatable baseline for the critical workflow and run it again after the change.
- Pin the .NET SDK and confirm that the target Windows Server, IIS and build machines can support it. For a web frontend, pin Node.js and pnpm.
- Set up the SQL project and a disposable development database. Document the build, DACPAC publish and EF scaffolding commands.
- Set up any React frontend with file-based routing, the shared UI primitives and a copy of the Prettier configuration below.
- Decide browser sign-in for each audience and identify which systems need app-to-app access.
- Document the existing IIS targets, versioned release process, verification commands and recovery procedure. Link this guide from the repository’s `AGENTS.md`.

## Backend architecture

Use the latest stable, supported .NET release when starting a new application. Pin the selected SDK in `global.json` and exclude prereleases. Confirm the target Windows Server supports the runtime and the matching ASP.NET Core IIS Hosting Bundle. If it cannot run the latest release, record a temporary exception for a supported version and a route to upgrade. The .NET Framework 4.8 target belongs to the SSDT database project. [Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) · [ASP.NET Core on IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)

Record who owns runtime and tooling updates. Review available .NET SDK, runtime and IIS Hosting Bundle patches monthly, prioritise security fixes and apply the tested versions through the release process. Schedule the next runtime upgrade before the current version reaches end of support.

Use onion architecture. Keep business policy in the inner projects and compose the implementation in the API host. Business and persistence dependencies point toward contracts and domain types.

| Project       | Responsibility                                                                                                                                                                            |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Domain`      | Plain domain and entity types, DTOs, expected errors and service, repository or provider contracts. Keep EF Core and HTTP dependencies out. FluentResults may appear in result contracts. |
| `Business`    | Use cases, rules, authorisation, orchestration and entity-to-DTO mapping. Depend on Domain contracts and keep EF Core, SQL clients and DataAccess references out.                         |
| `DataAccess`  | Repositories, queries, unit-of-work implementations and external provider adapters. Implement Domain contracts using Persistence or other infrastructure.                                 |
| `Persistence` | EF Core contexts and mappings for the owned schema. Keep generated output separate from handwritten extensions.                                                                           |
| `API`         | ASP.NET Core controllers, HTTP contracts, authentication, authorisation policies, request validation, response mapping and dependency registration.                                       |
| `Database`    | SSDT schema, publish profile and database scripts. Produces the DACPAC.                                                                                                                   |

Expose use cases through Business interfaces. Keep tracked entities, `DbContext`, `IQueryable` and provider SDK types inside the layers that own them. For larger apps, use `Modules/<Module>/` when an area owns business rules and contracts of its own. Use another module’s Business contract and DTOs for cross-module workflows. Add `Shared/` for genuinely shared primitives or infrastructure.

### Backend folder example

This example follows Agenda7’s onion project layout. A smaller application can use one `Core/` area and omit `Modules/`. Within each layer, group types by feature and responsibility. In the Appointments business contracts, `Services/` holds service interfaces and `Dtos/` holds the data those interfaces exchange.

```text
Example.slnx
Example.Api/
  Controllers/
    V1/
      AppointmentsController.cs
  Contracts/
    Http/
      V1/
        Appointments/
          Requests/
            CreateAppointmentRequest.cs
  Configuration/
  Errors/
    ResultProblemDetailsMapper.cs
Example.Api.Test/
  Controllers/
    V1/
      AppointmentsControllerTests.cs
Core/
  Example.Core.Domain/
    Contracts/
      Business/
        Appointments/
          Services/
            IAppointmentService.cs
          Dtos/
            AppointmentDto.cs
      DataAccess/
        Appointments/
          Repositories/
            IAppointmentRepository.cs
    Entities/
      Generated/
        Appointment.cs
  Example.Core.Business/
    Services/
      Appointments/
        AppointmentService.cs
    Rules/
      Appointments/
        AppointmentBookingRules.cs
    Mapping/
      Appointments/
        AppointmentMapper.cs
  Example.Core.Business.Test/
    Services/
      Appointments/
        AppointmentServiceTests.cs
  Example.Core.DataAccess/
    Repositories/
      Appointments/
        AppointmentRepository.cs
  Example.Core.DataAccess.Test/
    Repositories/
      Appointments/
        AppointmentRepositoryTests.cs
  Example.Core.Persistence/
    Generated/
      ExampleDbContext.cs
Example.Database/
  Example.Database.sqlproj
  Schemas/
    Bookings/
      Tables/
        Appointment.sql
  Scripts/
Example.Web/
```

Match handwritten C# namespaces to folders. Use one class or record per file, with its name as the filename. Keep meaningful tests next to the projects they cover and mirror their feature folders. Add folders and projects when there is code to put in them.

### HTTP APIs

- Use ASP.NET Core controller APIs with versioned routes such as `/api/v1/appointments`. Keep controllers thin. Bind and validate the HTTP request, invoke a Business interface and map the result to a response.
- Use FluentResults for expected Business failures. Give failures stable codes and safe messages. Use a shared host mapper to return Problem Details with the appropriate status. Return the unwrapped DTO on success. Preserve normal exception and cancellation handling for unexpected failures and cancellation.
- Treat the versioned OpenAPI document as the HTTP contract. Describe request and response types, auth requirements, expected statuses and stable error codes. Pin `openapi-typescript` in the frontend and generate contract types from the built API’s OpenAPI artifact. Keep handwritten form validation schemas alongside the feature. Use the generator’s `--check` mode or regenerate and compare the output in CI. [openapi-typescript CLI](https://openapi-ts.dev/cli)
- Expose Scalar and its browsing OpenAPI endpoint in the Development host. Generate the OpenAPI artifact during build or CI for contract checks and client generation.

### API compatibility

Retain the exact OpenAPI document shipped for each supported API version with its release artifacts. In CI, compare the candidate document for that version with its last released contract. Block breaking changes within a supported version. Also test relied-on behaviour that a schema comparison cannot detect, such as authorisation, validation, status and error semantics, and ordering where callers depend on it.

Use a new API version for a breaking change. Record known consumers and their owners, the migration and overlap plan, and how the team will verify that callers have moved before retiring the old version.

## SQL Server and EF Core

Use SQL Server for application data. Choose the SQL Server target platform to match the installations the application supports. Keep schema definitions in a classic SSDT `.sqlproj` targeting .NET Framework 4.8 and build the DACPAC with Visual Studio MSBuild and SSDT on Windows. Organise SQL files by owning schema and object type. [SQL database projects](https://learn.microsoft.com/en-us/sql/tools/sql-database-projects/sql-database-projects)

1. Change the declarative SQL objects in the database project. Add a versioned SQL script when existing data must move or change.
2. Build the DACPAC and publish it to an isolated development database.
3. Run a checked-in EF Core scaffolding script against the owned database objects. Put plain generated entities under `Domain/Entities/Generated/` and generated context and mapping code under `Persistence/Generated/`. Keep customisations outside generated files.
4. Update the affected queries, contracts and application code. Verify a clean database build and the upgrade path when an existing installation is affected.

Keep schema deployment, reference-data seeding and data migration as explicit operations. Make seeds safe to rerun. Deploy schema before starting the application and keep EF migrations, `EnsureCreated` and startup DDL out of the schema workflow. If the app uses ASP.NET Core Identity with SQL Server, manage its tables in the same SQL project. [EF Core reverse engineering](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/)

### EF boundary and scaffolding checks

Generated entities in Domain are plain database-shaped classes. Keep EF attributes, context configuration and Fluent API mappings in Persistence. Configure the scaffold script to use Fluent API mapping and suppress generated connection configuration. External API contracts use DTOs that Business maps explicitly.

DataAccess owns EF queries and change tracking. Use projections or no-tracking queries for read-only operations. Use tracked changes within the repository or unit-of-work boundary for writes, and commit at the explicit unit-of-work boundary. Business uses Domain contracts and receives detached data for reads. [EF Core tracking and no-tracking queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)

Pin the scaffold tooling. When a change affects a scaffolded object, CI should deploy the built DACPAC to disposable SQL, run the checked-in scaffold command and compare the generated files with the committed output. Record the included schemas or tables and normalise output that would otherwise vary between runs.

### Database upgrade and recovery

- Rehearse the upgrade from each supported starting version on representative data. Exercise the documented recovery path and check data integrity after the upgrade.
- Put versioned data scripts in the documented pre- or post-publish sequence. Record successful script versions in a migration journal. Use a transaction where practical and a restart checkpoint for work that can partially complete. Keep reruns safe, including a retry after interruption.
- Keep `BlockOnPossibleDataLoss=True` and `DropObjectsNotInSource=False` in the shared SqlPackage publish profile. Review the deployment report against the intended database. Handle a deliberate destructive change in a separately reviewed script or publish plan. [SqlPackage publish properties](https://learn.microsoft.com/en-us/sql/tools/sqlpackage/sqlpackage-publish)
- Prefer compatible changes that add the new schema first and remove old objects in a later release after consumers have moved. Verify whether the previous app release can run against the new schema. Where the versions cannot coexist, take affected writers offline and document the forward repair or database recovery path before deploying.

## React and TypeScript

Use React function components and hooks, strict TypeScript, Vite and pnpm. Pin the tooling and commit the lockfile. Give each library a clear responsibility.

| Library                    | Use                                                                       |
| -------------------------- | ------------------------------------------------------------------------- |
| TanStack Router            | Nested file-based routes, layouts, route guards and shareable URL state   |
| TanStack Query             | Server reads, mutations, cache keys and invalidation                      |
| TanStack Form and Zod      | Form state and validation against feature-owned schemas                   |
| TanStack Table             | Table state and behaviour, with the shared UI primitives for presentation |
| shadcn/ui and Tailwind CSS | UI primitives, composed components, theme and styling                     |

Keep route files thin. Use `route.tsx` for layouts and `index.tsx` for pages inside nested route folders. Put feature endpoints and query definitions in the feature’s `api/`, and keep view logic in its components and hooks. Keep generated API types separate from handwritten models. Use component state for local UI state, form state for form inputs, the URL for shareable navigation state and Query for server state. [TanStack file-based routing](https://tanstack.com/router/latest/docs/framework/react/routing/file-based-routing)

### Frontend folder example

```text
Example.Web/
  src/
    routes/
      __root.tsx
      _authenticated/
        route.tsx
        appointments/
          index.tsx
    features/
      appointments/
        api/
          endpoints.ts
          queries.ts
        model/
          appointment-form-schema.ts
        hooks/
          use-save-appointment.ts
        components/
          appointments-page.tsx
          appointment-form.tsx
    components/
      ui/
    generated/
    lib/
```

Use one handwritten component function per file. Use a descriptive component name and a kebab-case filename for feature components. A component’s props type can live beside it in the same file. Keep router and contract generator output intact.

### Start from shadcn/ui

Use shadcn/ui as the base for all UI components. Before writing a component, check the existing shared and feature components, then check the shadcn registry for the matching primitive or composition. Install or reuse the appropriate primitive under `src/components/ui/`. Keep related exports together when the registry ships them as a component family. Compose feature-specific behaviour on top and keep custom primitives for cases the existing components and registry cannot cover.

- Use **Calendar** or the **Date Picker** composition for date selection. For a bespoke scheduling view, reuse those controls for navigation and date selection where they fit.
- Use **Select** for a choice from a fixed list, **Combobox** for a searchable choice and **Dropdown Menu** for actions.
- Verify keyboard operation, focus, labels and validation messages when composing or adapting primitives.

[shadcn/ui component registry](https://ui.shadcn.com/docs/components)

## Code style and formatting

### C# conventions

Use a root `.editorconfig` based on Agenda7. Use UTF-8, LF, a final newline and four-space C# indentation. Put braces on a new line and use braces for control flow. Put `using` directives outside the namespace and sort System directives first. Use explicit accessibility, `var` for locals and block-bodied methods. Use 220 columns as a soft guide and wrap at meaningful boundaries.

Use PascalCase for types and members, camelCase for parameters and locals and an underscore prefix for private fields. Use descriptive names and explicit control flow.

#### XML documentation

Give every handwritten C# method complete XML documentation, including interface methods, constructors, non-public helpers and test methods. Keep shared documentation on the interface or base declaration. An implementation or override may use `<inheritdoc />` when it inherits the complete contract. Add implementation-specific details where they apply. Document controller actions for callers and Scalar.

- Use `<summary>` to describe what the method does. Include significant side effects or persistence responsibility when they are part of the contract. For a test method, state the behaviour it verifies.
- Use `<param>` for every parameter, including what it means and any relevant constraints. Use `<typeparam>` for each type parameter.
- Use `<returns>` when the method returns a value or an awaitable. Describe what the caller receives or what awaiting completion guarantees. Explain meaningful `null`, empty and boolean results.
- Use `<exception>` for known exceptions that can reach the caller and explain when they occur. Describe expected FluentResults failure codes and their conditions in `<remarks>`. Use remarks for other contract details such as ordering, transaction boundaries and concurrency guarantees.

### Comments and line spacing

Apply these rules to handwritten C# and TypeScript.

- Add a short line comment when the reason for a choice is unclear. Explain the business rule, edge case or ordering constraint a reader needs to understand. Give a complex expression a descriptive intermediate name and comment where its reasoning still needs explanation.
- Break large methods and other multi-step blocks into logical stages. Put a short, plain sentence above each stage that explains the work that follows. Omit labels and numbering such as `Stage:` or `Step 1:`.
- Use a single blank line between distinct groups of statements or expressions, such as validation, lookup, transformation, persistence and response. Separate independent conditionals and consecutive loops. Keep closely related assignments, expressions and calls together, and keep each stage comment adjacent to its block.

```csharp
// Reserve capacity before writing the booking so concurrent requests cannot take the same slot.
var reservation = await capacity.ReserveAsync(slotId, cancellationToken);
var booking = Booking.Create(reservation, customerId);

// Commit the booking and its reservation together.
await bookings.AddAsync(booking, cancellationToken);
await unitOfWork.CommitAsync(cancellationToken);

// Notify the customer once the booking is committed.
await notifications.SendConfirmationAsync(booking.Id, cancellationToken);
```

### Prettier

Put this Agenda7 baseline in `.prettierrc.json`. Install Prettier and `prettier-plugin-tailwindcss` as pinned development dependencies. Adjust `tailwindStylesheet` if the app uses a different entry file. Keep generated router and OpenAPI files, build output and coverage out of the formatting scope via `.prettierignore`. Run `pnpm exec prettier --check .` from the frontend directory in CI.

```json
{
    "singleQuote": true,
    "semi": true,
    "tabWidth": 4,
    "trailingComma": "all",
    "bracketSpacing": true,
    "arrowParens": "always",
    "endOfLine": "lf",
    "plugins": ["prettier-plugin-tailwindcss"],
    "tailwindStylesheet": "./src/index.css",
    "overrides": [
        {
            "files": "**/*.{js,cjs,mjs,ts,tsx,css,html}",
            "options": { "printWidth": 120 }
        }
    ]
}
```

### EditorConfig, Git and ESLint

Use this root `.editorconfig` as the starting point. Add project-specific naming and analyzer rules as needed, and keep generated files out of handwritten style checks.

```ini
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
indent_style = space
trim_trailing_whitespace = true

[*.cs]
indent_size = 4
max_line_length = 220
dotnet_sort_system_directives_first = true
dotnet_style_require_accessibility_modifiers = for_non_interface_members:warning
csharp_using_directive_placement = outside_namespace:warning
csharp_new_line_before_open_brace = all
csharp_prefer_braces = true:warning
csharp_style_var_for_built_in_types = true:warning
csharp_style_var_when_type_is_apparent = true:warning
csharp_style_var_elsewhere = true:warning
csharp_style_expression_bodied_methods = false:warning
csharp_style_expression_bodied_constructors = false:warning
csharp_style_expression_bodied_local_functions = false:warning

[*.md]
trim_trailing_whitespace = false

[packages.lock.json]
end_of_line = unset
insert_final_newline = false
```

Commit a root `.gitattributes` so Windows and CI check out the same text line endings. Add narrowly scoped exceptions for formats that require different handling.

```text
* text=auto eol=lf
*.bat text eol=crlf
*.cmd text eol=crlf
packages.lock.json -text -eol
```

Use an ESLint flat config for React applications. Pin `eslint`, `@eslint/js`, `typescript-eslint`, `eslint-plugin-react-hooks`, `globals` and `eslint-config-prettier`. Start with this `eslint.config.mjs` and adjust paths to match the project. Run `pnpm exec eslint . --max-warnings 0` from the frontend directory. [ESLint flat configuration](https://eslint.org/docs/latest/use/configure/configuration-files)

```javascript
import js from '@eslint/js'
import { defineConfig, globalIgnores } from 'eslint/config'
import prettier from 'eslint-config-prettier'
import reactHooks from 'eslint-plugin-react-hooks'
import globals from 'globals'
import tseslint from 'typescript-eslint'

export default defineConfig([
    globalIgnores(['dist/', 'coverage/', 'playwright-report/', 'test-results/', 'src/generated/', 'src/routeTree.gen.ts']),
    {
        files: ['**/*.{js,mjs,cjs}'],
        extends: [js.configs.recommended],
        languageOptions: { globals: globals.node },
    },
    {
        files: ['**/*.{ts,tsx}'],
        extends: [js.configs.recommended, tseslint.configs.recommended],
    },
    {
        files: ['src/**/*.{ts,tsx}'],
        extends: [reactHooks.configs.flat.recommended],
        languageOptions: { globals: globals.browser },
    },
    prettier,
])
```

## Authentication and inter-app communication

### Browser sign-in

Choose the sign-in methods per application and installation. Forms and Microsoft Entra ID are the common staff options. Support either or both where the audience needs them. Use ASP.NET Core Identity when the application owns Forms credentials and account lifecycle. Use ASP.NET Core’s OpenID Connect handler with authorisation code flow and PKCE for Entra, and configure the allowed tenant or directory for each installation. IIS Windows Authentication is an option for installations that require it.

For customer-facing accounts, prefer the customer’s approved OpenID Connect provider when available. Use ASP.NET Core Identity when the app owns customer credentials, recovery and MFA. Define which audience each sign-in entry point serves. [ASP.NET Core OpenID Connect](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-oidc-web-authentication) · [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)

Use a same-origin browser API and secure, HTTP-only application session cookies. Default the application session cookie to `SameSite=Lax` and verify sign-in and callbacks with the configured identity provider. Scope the cookie’s name and path and isolate persistent Data Protection keys by installation and environment. Verify these settings at the actual IIS path prefix. Validate access on the server, including resource-level permissions.

Expose a same-origin endpoint that issues the ASP.NET Core anti-forgery cookie and returns a request token with a no-store response. Keep the request token in SPA memory and send it in `X-CSRF-TOKEN` on state-changing browser requests. Refresh the token after sign-in or sign-out when the principal changes. Keep server credentials and service keys out of browser code and storage. [ASP.NET Core anti-forgery guidance](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery)

### App-to-app calls

Use the owning application’s HTTPS API with a versioned OpenAPI contract. Keep database connections scoped to the owning application. Prefer a typed HTTP client with explicit timeouts and a cancellation token. Retry only where the operation is safe to repeat or protected by an idempotency contract. Use asynchronous messaging where a workflow needs durable delivery or separate processing, and define how retries and duplicate messages are handled.

- Send a single `X-Api-Key: <key-id>.<secret>` header from the calling server. Keep the key out of query strings, frontend configuration, responses and logs.
- Issue separate high-entropy credentials per caller and environment. On the receiving server, require exactly one key header, look up the key ID, verify a hash of the secret in fixed time and authorise the caller’s permitted operations. Keep the raw key in protected configuration on the caller.
- Rotate by accepting the old and new keys briefly, switching the caller, verifying requests and revoking the old key. Coordinate existing header changes across both applications.
- On a host that serves browsers and machine callers, register separate named authentication schemes and authorisation policies. Require the application cookie on protected browser endpoints and the API-key scheme on machine endpoints. Keep machine routes in a distinct controller or route group.
- Apply anti-forgery validation to state-changing browser routes, including sign-in and sign-out where applicable. API-key-only routes use machine authorisation without the browser anti-forgery requirement. Verify in integration tests that each credential is accepted only by its intended policy.
- Describe the machine header in OpenAPI without supplying a real credential.

## Production diagnostics and logging

Use Serilog for all backend logging. Wire it into the ASP.NET Core host with `Serilog.AspNetCore` and use injected `ILogger<T>` in application code. Configure log levels and sinks per environment and capture startup failures. Use `UseSerilogRequestLogging()` before the API handlers to record a structured completion event with the HTTP method, route, status and elapsed time. Default to route templates or safe route identifiers in production. Exclude sensitive raw paths and query strings from structured event properties. [Serilog integration for ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)

Log frequently enough to follow a workflow from start to outcome from its logs. Add checkpoints at meaningful stages in multi-step requests, integrations, jobs and release scripts. Record branch outcomes, retries and failures with enough context to show where work stopped. Use structured message templates and properties so support can filter by operation, component, trace ID, release version, installation and environment. Add safe internal identifiers, elapsed time, counts and error codes where useful.

| Level       | What to record                                                                                                                            |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| Debug       | Routine progress through validation, lookup, persistence and external calls, expected rejections, and branch decisions useful for tracing |
| Information | Request completion, significant business milestones, job or batch outcomes and release or startup milestones                              |
| Warning     | Recoverable failures, retries, fallbacks, partial completion and unusual conditions that need attention                                   |
| Error       | Unexpected exceptions and failed requests, jobs or integrations that could not complete as intended                                       |
| Fatal       | Startup or host failures that prevent the application from running                                                                        |

Use job or request scopes so related events share context. Carry trace context across outbound HTTP calls and asynchronous work. Log an unexpected failure at the boundary that handles it, with the trace ID, the failed stage and an exception or safe diagnostic classification. Use summaries, counts and outcome codes for repetitive loop work. Redact credentials, tokens, cookies, connection strings, request payloads and sensitive personal data before they reach any sink. Keep diagnostic coverage useful when production runs at Information. Document how support can enable targeted Debug logging when needed.

### Health checks and production errors

- Provide a lightweight liveness check that shows the process can respond and a readiness check for dependencies required to serve traffic, such as the database. Use distinct paths, for example `/health/live` and `/health/ready`, relative to the API’s IIS mount. Return minimal status and give monitoring and deployment callers appropriate access. [ASP.NET Core health checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- Use central ASP.NET Core exception handling and Problem Details. Outside Development, return a safe message and trace ID for unexpected errors. Keep stack traces and provider details out of responses. Log diagnostic details under the same trace ID.
- Document where production Serilog events arrive, how to query them by trace ID and release version, how to view startup failures, retention, alert conditions and the support owner. Verify the configured sink receives production-level events during release checks.

## IIS deployment and release pipelines

Deploy a named version to an existing HTTPS IIS site on Windows Server. Host the static React app and ASP.NET Core API on sibling paths under the same origin by default. Put the API in a dedicated IIS app pool. Support installations mounted under a URL prefix. Use SPA fallback only for UI routes so that API and missing asset requests retain their normal error responses.

Supply environment settings at deployment. Keep public runtime configuration limited to non-secret values such as base paths. Keep installed server configuration and secrets separate from the application package. Validate the target site, application paths, bindings, app pool, database and required configuration before changing the installation.

### Release terms

- An **environment** is a promotion stage such as Test, Staging or Live. It may contain multiple installations.
- An **installation** is a specific deployed instance with its IIS targets, configuration, identities and database target. Record its environment, ownership and any shared resources.
- A **client variant** is a build-time difference, such as branding assets that must be bundled. Default to installation configuration for differences that can be applied at deployment. Record why each required build-time variant exists and which installations use it.

### Package and promote a release

1. Build on a self-hosted Windows worker with the pinned .NET SDK, Node.js, pnpm, Visual Studio MSBuild and SSDT as applicable. Run the repository’s verification checks.
2. Build one package from the reviewed `vX.Y.Z` tag for each required client variant. Include the API publish output, built SPA, DACPAC and any versioned database scripts. Record the tag, commit, variant and file hashes. Keep target-specific secrets and settings outside the package.
3. Promote the same verified package for the selected variant through Test, Staging and Live. Use a deployment worker scoped to the selected environment. Acquire the installation and affected database deployment locks before target preflight. Hold them through activation or recovery so concurrent runs, including runs from different CI platforms, cannot mutate the same target.
4. Review the database deployment plan and verify that a recoverable backup is available before applying schema changes. Follow the rehearsed database upgrade order. Take affected writers offline when required, deploy the app files and preserve installation-owned settings.
5. Smoke-test liveness and readiness, the SPA base path and a deep link. Verify sign-in, a representative application action and delivery of structured logs before activation. Document when to resume, redeploy the old application or perform a forward repair. After activation, account for new writes before any database restore.

Use a shared lock for a target that Azure DevOps and GitHub Actions can both deploy to. Acquire locks for shared resources in a consistent order. Record the owning release and define how the release operator verifies target state before clearing a stale lock after a failure.

### Azure DevOps and GitHub Actions

Use the same build, packaging and deployment scripts from either CI platform. Keep build workers separate from deployment workers. Only deployment workers have access to the target IIS and SQL environments.

| Workflow         | Azure DevOps                                                                                         | GitHub Actions                                                                                        |
| ---------------- | ---------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| Verify and build | YAML pipeline on a self-hosted Windows build agent pool                                              | Workflow under `.github/workflows/` on a self-hosted Windows build runner                             |
| Package          | Publish the tagged build as a pipeline artifact and retain it for promotion                          | Upload the tagged build as an Actions artifact and retain it for promotion                            |
| Deploy           | Deployment job targeting the selected Azure DevOps Environment using the self-hosted deployment pool | Deployment job targeting the selected GitHub environment using a scoped self-hosted deployment runner |
| Configuration    | Environment-scoped settings and secrets or protected installation configuration                      | Environment-scoped settings and secrets or protected installation configuration                       |

[Azure Pipelines agents](https://learn.microsoft.com/en-us/azure/devops/pipelines/agents/agents?view=azure-devops) · [GitHub self-hosted runners](https://docs.github.com/en/actions/concepts/runners/self-hosted-runners)

## Documentation

Keep documentation in the application repository and update it in the same change as the behaviour, configuration or workflow it describes. Use the root `README.md` as the entry point. Link from it to the quick start, developer guides and feature index or individual feature docs. Give each fact one home and link to it from related pages.

### Feature documentation

Use `Docs/Features/<feature>.md` or a feature subfolder for larger areas. Add a feature document when introducing a feature and update it when behaviour changes. Describe the feature from the reader’s point of view.

- State the purpose, who uses it, the permissions or prerequisites and the main workflow.
- Describe relevant rules, valid inputs, state changes and important failure or edge cases. Explain visible side effects such as messages, audit records or changes in other systems.
- Document feature flags, settings and integration dependencies where they affect behaviour. Include examples or verification steps when they help someone understand or check the outcome.

### Developer documentation

Use `Docs/Development/` for developer guides. Keep a short runnable quick start in `README.md` or link directly to it.

- Record required tools and setup, local configuration and where developers obtain secrets. Use placeholders in examples.
- Include commands for restoring, building, running, linting and testing. Document database creation, seeding and EF scaffolding, and how to regenerate API contracts and route files.
- Describe project structure, dependency boundaries and relevant architectural decisions. Include troubleshooting steps for common local failures.
- Link to the release and deployment runbook for environment settings, verification and recovery. Check command examples and links when the workflow changes.

## Verification and AI agent rules

### Verification baseline

Run the same checked-in verification commands locally and in CI on both delivery platforms. Pin tool versions, commit lockfiles and use locked restores in CI. Document the actual commands, prerequisites, local configuration and expected outputs in the repo’s developer guide.

- Enable nullable checking and `EnforceCodeStyleInBuild` for C# projects. Generate XML documentation when building, and surface XML syntax and parameter warnings. For format checks, run `dotnet format <project-or-solution> --verify-no-changes` against the relevant C# projects or solution, with generated files excluded. Review XML documentation for every method, including private helpers and tests that the build cannot check for coverage. [dotnet format options](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
- Use xUnit for new .NET tests, and run SQL integration tests against a disposable database for persistence and upgrades. Check generated EF output when schema changes affect scaffolded objects.
- Run TypeScript checking, ESLint and Prettier for the frontend. Verify generated OpenAPI and route output and check compatibility with the released API contract. Use Vitest for frontend unit tests and Playwright for critical browser journeys.

### AI agent rules

Keep a short root `AGENTS.md` with the central guide link, local architecture references, setup and verification commands, generated file locations, sensitive operations and agreed exceptions.

- Read the applicable instructions and project references before changing a feature. Follow the existing dependency boundaries and local decisions.
- Check existing components and the shadcn registry before creating UI primitives. Explicitly check Calendar, Date Picker, Select, Combobox and Dropdown Menu for tasks that use those controls.
- Make schema changes in the SQL project and regenerate EF output. Regenerate OpenAPI clients and route trees with the repo’s commands.
- Keep the work focused on the requested change. Carry existing user changes forward and avoid including credentials or sensitive runtime values in commits, logs or handoffs.
- Update affected feature and developer docs alongside code changes. Check linked references and command examples that the change touches.
- Run checks that cover the change and report what ran, the outcome and any checks still outstanding. Use local or disposable targets for routine verification and follow the repo’s rules for shared or live operations.
- Record departures from this guide with the affected scope, reason and review or migration trigger. Keep the repo’s instructions current when commands or conventions change.

## Reference implementations

These are examples from the application survey. Where an existing app makes a different decision, use the guide above as the shared default for new work.

| Reference          | Where to look                                                                                                                                                                                                                                               |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Agenda7            | `Agenda.slnx` and `Docs/architecture.md` for onion layers, `Docs/Adr/0003-sql-first-coordinated-database.md` for the database workflow, `Docs/frontend.md` and `Agenda.Admin/.prettierrc.json` for the frontend, and `Pipelines/README.md` for IIS releases |
| Customer CRM       | `Docs/engineering-standards.md` for controller, FluentResults and React conventions, and `Docs/deployment.md` for tagged IIS release checks                                                                                                                 |
| BinOrdering        | `docs/coding-standards.md` for SQL-first ownership and `docs/customer-portal.md` for machine credentials and rotation                                                                                                                                       |
| TeamsAPI           | `README.md` for a compact controller API with Development Scalar, the `X-Api-Key` header and IIS setup                                                                                                                                                      |
| WasteCentreCheckIn | `CheckInAPI/` and `web/` for a smaller .NET and React application using the TanStack stack                                                                                                                                                                  |
