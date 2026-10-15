# Waypoint

Waypoint is a youth leadership platform for Bible study, mentoring, leadership development, and coding.

[![Backend CI](https://github.com/QuinntyneBrown/waypoint/actions/workflows/backend.yml/badge.svg)](https://github.com/QuinntyneBrown/waypoint/actions/workflows/backend.yml)
[![Frontend CI](https://github.com/QuinntyneBrown/waypoint/actions/workflows/frontend.yml/badge.svg)](https://github.com/QuinntyneBrown/waypoint/actions/workflows/frontend.yml)
[![Performance](https://github.com/QuinntyneBrown/waypoint/actions/workflows/performance.yml/badge.svg)](https://github.com/QuinntyneBrown/waypoint/actions/workflows/performance.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

[Getting started](#getting-started) · [Documentation](#documentation) · [Contributing](#contributing) · [Support](SUPPORT.md)

> [!WARNING]
> Waypoint remains a development project. Independent security, privacy, safeguarding, and accessibility reviews have not been completed, and no release is approved for production use or for storing real participant information. Use synthetic data only. See the current [implementation status](docs/implementation-plan.md) and [security policy](SECURITY.md).

## Overview

Waypoint is designed to bring youth applications, consent and safeguarding, cohort operations, learning, mentoring, attendance, communication, and reporting into one role-aware system. It serves youth, mentors, guardians, volunteers, program administrators, and public visitors.

The repository contains a .NET 9 API and operational CLI, an Angular 21 authenticated portal and public SSR site, shared frontend libraries, SQL Server persistence, automated acceptance tests, and static interaction references. The product scope comes from the [product requirements](docs/PRD.md) and 88 in-scope, acceptance-testable requirements under [`docs/specs`](docs/specs/README.md).

## Capabilities

- Public program content, youth applications, contact requests, volunteer sign-up, and donation pledges
- Role-specific portal experiences for youth, mentors, guardians, volunteers, and program administrators
- Bible, leadership, and coding curricula with lessons, reflections, quizzes, assignments, progression, and badges
- Cohort, curriculum, schedule, attendance, mentoring, communication, and reporting workflows
- Guardian consent, safeguarding gates, scoped authorization, privacy requests, and auditable sensitive actions
- Invitations, password recovery, notifications, realtime updates, and durable email delivery
- Responsive browser experiences with automated route, accessibility, performance, and requirements-traceability gates

## Getting started

### Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/) (selected by [`global.json`](global.json))
- [Node.js](https://nodejs.org/) `^20.19.0`, `^22.12.0`, or `>=24.0.0`
- npm 10.9.4
- SQL Server only for integration tests or persistent local data; the default development setup uses an in-memory database

### Set up the repository

```bash
git clone https://github.com/QuinntyneBrown/waypoint.git
cd waypoint
dotnet restore backend/Waypoint.sln
npm --prefix frontend ci
```

### Run locally

Start each process from the repository root in a separate terminal.

Terminal 1 — API:

```bash
dotnet run --project backend/src/Waypoint.Api --environment Development --urls http://127.0.0.1:5211
```

Terminal 2 — authenticated portal:

```bash
npm --prefix frontend start
```

Terminal 3 — public site:

```bash
npm --prefix frontend run start:public
```

| Service              | Local URL                            |
| -------------------- | ------------------------------------ |
| Authenticated portal | `http://localhost:4200`              |
| Public site          | `http://localhost:4201`              |
| API documentation    | `http://127.0.0.1:5211/swagger`      |
| Health checks        | `http://127.0.0.1:5211/health`       |
| Readiness check      | `http://127.0.0.1:5211/health/ready` |

Both web applications proxy `/api` and `/hubs` to the API. Development starts with an empty, process-local database and does not create demo accounts outside the `Testing` environment. This quick start lets you inspect the API, browse the public site, and reach the portal sign-in screen; it does not bootstrap an authenticated user. The HTML files under [`docs/mocks`](docs/mocks/README.md) are interaction references, not the running application.

### Configuration

Local defaults are defined in [`appsettings.json`](backend/src/Waypoint.Api/appsettings.json). ASP.NET Core configuration keys use double underscores in environment variables; for example, `Jwt:SigningKey` becomes `Jwt__SigningKey`.

| Configuration                                                        | Purpose                                                                                  |
| -------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `ConnectionStrings:Default`                                          | Uses `InMemory` locally; provide a SQL Server connection string for durable persistence. |
| `Jwt:SigningKey`                                                     | Signs access tokens. Replace the development placeholder outside local use.              |
| `AuditIntegrity:SigningKey`                                          | Protects audit-record integrity. Replace the development placeholder.                    |
| `SensitiveData:EncryptionKey`                                        | Protects sensitive stored values. Replace the development placeholder.                   |
| `ApplicationUrls:PortalBaseUrl`, `ApplicationUrls:PublicSiteBaseUrl` | Build absolute application links; production values must use HTTPS.                      |
| `Smtp:*`                                                             | Configures SMTP delivery, TLS, sender identity, retry limits, and optional credentials.  |

Production startup rejects the in-memory database, placeholder keys, non-HTTPS application URLs, and missing required SMTP settings. Keep secrets outside source control. See the [architecture guide](docs/ARCHITECTURE.md) and [CLI guide](backend/src/Waypoint.Cli/README.md) for operational details.

## Architecture

| Area           | Implementation                                                                                    |
| -------------- | ------------------------------------------------------------------------------------------------- |
| Backend        | ASP.NET Core on .NET 9, MediatR/CQS, FluentValidation, and SignalR                                |
| Persistence    | Entity Framework Core with SQL Server and an in-memory development provider                       |
| Authentication | JWT access tokens, rotating refresh tokens, BCrypt password hashing, and role-based policies      |
| Frontend       | Angular 21 portal, Angular SSR public site, RxJS, and TypeScript                                  |
| Quality        | NUnit, `WebApplicationFactory`, Vitest, Playwright, axe, load gates, and requirement traceability |

The backend follows a Clean Architecture dependency direction: API → Application → Domain, with Infrastructure implementing Application ports. Read [the architecture documentation](docs/ARCHITECTURE.md) before changing module boundaries, persistence, authorization, or data flow.

## Development and testing

Run backend and requirements checks from the repository root:

```bash
dotnet tool restore
dotnet restore backend/Waypoint.sln
dotnet build backend/Waypoint.sln --configuration Release --no-restore
dotnet ef migrations has-pending-model-changes --project backend/src/Waypoint.Infrastructure --startup-project backend/src/Waypoint.Api --configuration Release --no-build
dotnet test backend/Waypoint.sln --configuration Release --no-build
node scripts/check-traceability.mjs --strict
```

Integration tests create disposable SQL Server databases. They use `(localdb)\MSSQLLocalDB` by default on Windows. On Linux or macOS, provision a reachable SQL Server and set `WAYPOINT_TEST_SQLSERVER` and `WAYPOINT_TEST_SQLAUTH`; the [backend CI workflow](.github/workflows/backend.yml) shows a known-good container and connection setup.

Run frontend checks from `frontend/`:

```bash
cd frontend
npm ci
npm run format:check
npm run quality:routes
npm run build
npm test -- --watch=false

# Browser acceptance, responsive, accessibility, and interaction-performance gates
npm run api:build
npx playwright install chromium
npm run e2e:ci
npm run e2e:performance
```

`npm run api:build` creates the Debug API build expected by the Playwright web server on a clean checkout. The [backend](.github/workflows/backend.yml), [frontend](.github/workflows/frontend.yml), and [performance](.github/workflows/performance.yml) workflows define the authoritative CI gates, including the SQL-backed API load profile. See [CONTRIBUTING.md](CONTRIBUTING.md) for the development workflow and change-specific test expectations.

## Repository layout

```text
backend/
  src/                 Domain, Application, Infrastructure, API, and operational CLI
  tests/               Unit and SQL-backed integration tests
frontend/
  projects/portal/     Authenticated role-aware application
  projects/public-site/ SSR public site and intake experience
  projects/w-api/     Typed API client and models
  projects/w-auth/    Session state, guards, and authentication integration
  projects/w-ui/      Shared design system and application shell
  e2e/                 Playwright acceptance and quality gates
docs/
  specs/               L1/L2 requirements and acceptance criteria
  detailed-designs/    Feature-level designs and diagrams
  mocks/               Static interaction references
scripts/               Traceability, documentation, and performance tooling
```

## Documentation

| Document                                             | Purpose                                                           |
| ---------------------------------------------------- | ----------------------------------------------------------------- |
| [Implementation status](docs/implementation-plan.md) | Implemented boundary, verification commands, and release work     |
| [Product requirements](docs/PRD.md)                  | Product goals, users, scope, and constraints                      |
| [Requirements specification](docs/specs/README.md)   | Stable requirement IDs, acceptance criteria, and traceability     |
| [Architecture](docs/ARCHITECTURE.md)                 | System layers, request flow, persistence, and security boundaries |
| [Architecture decisions](docs/DECISIONS.md)          | Durable technical and product decisions                           |
| [Frontend guide](frontend/README.md)                 | Angular applications, libraries, and frontend commands            |
| [Operational CLI](backend/src/Waypoint.Cli/README.md)  | CLI commands and production configuration                         |
| [Design references](docs/mocks/README.md)            | Static product map and role-based interaction references          |
| [Waypoint in pictures](docs/waypoint-in-pictures.html) | Plain-language picture explainer of what the repository does and how it is built; open the file locally in a browser |
| [Changelog](CHANGELOG.md)                            | Notable contributor-visible changes                               |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a pull request, and open an issue before large features or changes involving schemas, authorization, consent, safeguarding, or participant data. All participants must follow the [Code of Conduct](CODE_OF_CONDUCT.md).

Contributors are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md). Maintainer roles and decision making are documented in [GOVERNANCE.md](GOVERNANCE.md).

## Support

For usage questions and non-sensitive problems, read [SUPPORT.md](SUPPORT.md) and search the [existing issues](https://github.com/QuinntyneBrown/waypoint/issues). Maintainers and contributors provide support on a best-effort basis without a service-level agreement.

## Security

Do not report vulnerabilities in a public issue. Follow [SECURITY.md](SECURITY.md) for the project's private reporting path. The latest commit on `main` is the only supported code line; older commits, forks, and deployments are not supported.

## License

Copyright (c) 2026 Waypoint contributors. Released under the [MIT License](LICENSE).
