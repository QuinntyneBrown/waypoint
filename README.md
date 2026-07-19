<div align="center">

# Word Up

A youth leadership platform for helping young people grow as **leaders who love Jesus** through Bible study, mentoring, leadership development, and coding.

[![Backend CI](https://github.com/QuinntyneBrown/word-up/actions/workflows/backend.yml/badge.svg)](https://github.com/QuinntyneBrown/word-up/actions/workflows/backend.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-c6fb50.svg)](LICENSE)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Code style: Prettier](https://img.shields.io/badge/code_style-Prettier-F7B93E?logo=prettier&logoColor=black)](https://prettier.io/)
[![Contributions welcome](https://img.shields.io/badge/contributions-welcome-1d8fb9.svg)](CONTRIBUTING.md)

[Implementation plan](docs/implementation-plan.md) | [Architecture](docs/ARCHITECTURE.md) | [Requirements](docs/specs/README.md) | [Design mocks](docs/mocks/README.md) | [Contributing](CONTRIBUTING.md)
</div>

## About the project

Word Up supports a Toronto youth program serving communities across Rexdale, Warden, and St. Clair. The product brings applications, safeguarding, cohort administration, learning, mentoring, attendance, communications, and progress reporting into one role-aware platform, replacing workflows currently spread across spreadsheets and manual follow-up.

The repository is being built from an acceptance-testable requirements set. Its current implementation is an early full-stack foundation: a .NET 9 API vertical slice, an Angular 21 workspace awaiting its first application, and roughly 115 static HTML screens used as the interaction and visual reference. The backend currently covers identity and access, invitations, application intake, onboarding, password recovery, guardian consent, role policies, and audit records.

Word Up uses synthetic development and test data and has not completed an independent production security, privacy, safeguarding, or accessibility assessment. It is not ready to store real participant information or operate as a system of record. See the [implementation plan](docs/implementation-plan.md) for delivery status and sequencing.

## Features

- Email and password authentication with JWT access tokens and rotating refresh tokens
- Account lockout, password recovery, invitation expiry, and role-based authorization policies
- Youth application intake with administrator approval and decline flows
- Guardian consent capture, withdrawal, status checks, and safeguarding gates
- Resumable onboarding for invited participants and staff
- Auditable sensitive actions with role-restricted, redacted audit-log access
- A requirements corpus spanning 14 subsystems and 88 detailed acceptance-testable requirements
- Automated requirement traceability checks for acceptance tests
- Roughly 115 responsive, offline-capable HTML design mocks covering all product roles
- An Angular 21 workspace reserved for the production web client

See [docs/mocks](docs/mocks/README.md) for the FaithTech-styled interaction reference and [docs/specs](docs/specs/README.md) for the product requirements.

## Getting started

### Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/) (pinned in [backend/global.json](backend/global.json))
- [Node.js](https://nodejs.org/) 20 or later and npm
- SQL Server for integration tests; LocalDB is the default on Windows, and environment variables can target another instance

### Local development

```bash
git clone https://github.com/QuinntyneBrown/word-up.git
cd word-up

# Run the API with its default in-memory development database
dotnet run --project backend/src/WordUp.Api -e ASPNETCORE_ENVIRONMENT=Development --urls http://localhost:5099
# Swagger: http://localhost:5099/swagger
# Health:  http://localhost:5099/health

# Install the Angular workspace dependencies
cd frontend
npm install
```

The Angular workspace does not yet define an application or runnable target. Until that vertical slice is built, open [`docs/mocks/index.html`](docs/mocks/index.html) directly in a browser to explore the product screens.

The API uses an in-memory database when `ConnectionStrings:Default` is `InMemory`. Supply a SQL Server connection string through configuration for persistent environments. Never commit secrets: the development `Jwt:SigningKey` is a placeholder and must be replaced through user secrets or environment-specific configuration before any non-local use.

## Technology

| Area             | Technologies                                                                                      |
| ---------------- | ------------------------------------------------------------------------------------------------- |
| Backend          | .NET 9, ASP.NET Core, MediatR / CQS, FluentValidation                                             |
| Persistence      | Entity Framework Core, SQL Server, in-memory development provider                                 |
| Authentication   | JWT bearer tokens, rotating refresh tokens, BCrypt                                                |
| Frontend         | Angular 21 workspace, RxJS, TypeScript 5                                                          |
| Design reference | Static HTML, CSS, and JavaScript using the FaithTech design language                              |
| Quality          | xUnit, `WebApplicationFactory`, GitHub Actions, requirements traceability, Prettier, EditorConfig |

## Testing

```bash
# Restore, build, and run the backend unit and integration tests
dotnet test backend/WordUp.sln --configuration Release

# Verify that test trace headers reference valid detailed requirements
node scripts/check-traceability.mjs
```

Integration tests provision a disposable SQL Server database. They default to `(localdb)\MSSQLLocalDB`; set `WORDUP_TEST_SQLSERVER` and, when needed, `WORDUP_TEST_SQLAUTH` to use another instance. The Angular workspace has no project target yet, so frontend build and test commands will become available when the first application is generated.

## Project structure

```text
backend/
  src/WordUp.Domain/               Entities and lifecycle state grouped by subsystem
    <Subsystem>/                    Subsystem-owned domain types and namespaces
  src/WordUp.Application/          Use cases grouped by subsystem, then feature
    <Subsystem>/<Feature>/         Commands, queries, handlers, and validators
    Shared/                         Cross-cutting ports, behaviors, and exceptions
  src/WordUp.Infrastructure/       EF Core, tokens, hashing, email, and audit services
  src/WordUp.Api/                  Policies, middleware, and composition root
    Controllers/<Subsystem>/       HTTP controllers only
    Contracts/<Subsystem>/         HTTP request contracts
  tests/WordUp.UnitTests/          Isolated validation and domain-level tests
  tests/WordUp.IntegrationTests/   Full HTTP tests over disposable SQL Server databases
frontend/                          Angular 21 workspace; application implementation pending
docs/
  PRD.md                           Product requirements source
  specs/                           L1/L2 subsystem requirements and acceptance criteria
  implementation-plan.md           Delivery phases and current status
  backend-implementation-plan.md   Detailed backend phase-one plan
  mocks/                           Static product map, design system, and role-based screens
scripts/
  check-traceability.mjs           Requirement-to-test reference validator
```

## Documentation

| Document                                                           | Purpose                                                           |
| ------------------------------------------------------------------ | ----------------------------------------------------------------- |
| [Implementation plan](docs/implementation-plan.md)                 | Delivery sequence, current status, and verification gates         |
| [Backend implementation plan](docs/backend-implementation-plan.md) | Detailed plan for the initial API foundation                      |
| [Architecture](docs/ARCHITECTURE.md)                               | System layers, request flow, persistence, and security boundaries |
| [Architecture decisions](docs/DECISIONS.md)                        | Technical and product decisions worth recording                   |
| [Product requirements](docs/PRD.md)                                | Product goals, users, scope, and constraints                      |
| [Requirements specification](docs/specs/README.md)                 | L1/L2 requirement organization, IDs, and traceability rules       |
| [Design mocks](docs/mocks/README.md)                               | Static FaithTech-styled interaction and visual reference          |
| [Page-builder contract](docs/contract.md)                          | Shared HTML, CSS, and JavaScript conventions for mock screens     |

## Contributing

Contributions are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) for the development workflow, branch conventions, quality gates, requirements traceability, and architecture boundaries. Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

The people who help build Word Up are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md), and notable changes are recorded in [CHANGELOG.md](CHANGELOG.md).

## Security

Do not report vulnerabilities in a public issue. Follow [SECURITY.md](SECURITY.md) to submit a private report. For usage questions and non-sensitive problems, see [SUPPORT.md](SUPPORT.md).

## Governance

Maintainer responsibilities, decision making, and the path to becoming a maintainer are described in [GOVERNANCE.md](GOVERNANCE.md).

## License

Copyright (c) 2026 Word Up contributors. Released under the [MIT License](LICENSE).
