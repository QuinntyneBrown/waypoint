# Contributing to Word Up

Thank you for helping improve Word Up. Contributions may include code, tests, design, documentation, issue triage, accessibility feedback, safeguarding insight, and product knowledge.

By participating, you agree to follow the [Code of Conduct](CODE_OF_CONDUCT.md). For support or security concerns, use the channels in [SUPPORT.md](SUPPORT.md) and [SECURITY.md](SECURITY.md).

## Before contributing

- Search existing issues and pull requests before starting work.
- Open an issue before large features, schema changes, authorization changes, or changes that affect consent, safeguarding, or participant data.
- Read the relevant L1 and L2 requirements in [`docs/specs`](docs/specs/README.md). Product behaviour must remain traceable to the acceptance criteria.
- Never include real participant information, credentials, signing keys, connection strings, or unredacted logs in issues, fixtures, screenshots, commits, or tests. Use synthetic data.

## Development setup

```bash
git clone https://github.com/QuinntyneBrown/word-up.git
cd word-up

# Backend
dotnet run --project backend/src/WordUp.Api --urls http://localhost:5099

# Angular workspace dependencies
cd frontend
npm install
```

See the [README](README.md) for prerequisites, current project status, and persistence configuration. The Angular workspace does not yet define a runnable application target; the static product reference lives in [`docs/mocks`](docs/mocks/README.md).

## Development workflow

1. Create a branch from the latest `main` using `feat/`, `fix/`, `docs/`, or `chore/` followed by a short description.
2. Keep the change focused and avoid unrelated formatting or refactoring.
3. For behaviour changes, start from the owning L2 acceptance criteria and add or update a traced test before implementing the change.
4. Put business rules in the Domain or Application layer. Controllers translate HTTP requests and must not become the source of authorization, safeguarding, or lifecycle rules.
5. Documentation-only, repository metadata, configuration, and template changes do not require runtime tests unless they change executable behaviour.
6. Update architecture, decisions, requirements, or changelog documentation when public behaviour or contributor workflow changes.
7. Open a pull request and respond to review feedback.

Do not commit directly to `main`. Maintainers merge approved pull requests after the relevant checks pass.

## Quality gates

Run the checks relevant to your change:

```bash
# Backend build and tests
dotnet build backend/WordUp.sln --configuration Release
dotnet test backend/WordUp.sln --configuration Release --no-build

# Requirement references
node scripts/check-traceability.mjs
```

Integration tests require SQL Server. They use `(localdb)\MSSQLLocalDB` by default; override the target with `WORDUP_TEST_SQLSERVER` and `WORDUP_TEST_SQLAUTH` when necessary.

The frontend workspace has no application target yet. Once an application is introduced, its build, unit-test, accessibility, and end-to-end commands become required gates for relevant changes.

## Requirements traceability

- Every acceptance test must include `// Traces to: L2-SUBSYSTEM-NNN` for each requirement it verifies.
- Do not invent or reuse requirement IDs. Follow the stable-ID rules in [`docs/specs/README.md`](docs/specs/README.md).
- A test name is not sufficient traceability; the trace comment is the machine-readable contract.
- Update an owning L2 document and its acceptance criteria before intentionally changing required behaviour.

## Architecture boundaries

Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before changing module boundaries or data flow. Key conventions:

- Dependency direction is `Api → Application → Domain`; Infrastructure implements abstractions declared by Application.
- Domain and Application code must not depend on ASP.NET Core transport concerns.
- Commands and queries flow through MediatR handlers; controllers remain thin.
- Authentication, authorization, consent, and participant scoping are enforced on the server and covered by tests.
- Database changes require a forward migration once persistent production storage is introduced and, where relevant, an architecture-decision update.

## Frontend and mock contributions

- Treat [`docs/mocks`](docs/mocks/README.md) as the current interaction and visual reference, not as production application code.
- Follow [`docs/contract.md`](docs/contract.md) when changing mock pages so shared navigation, CSS classes, accessibility patterns, and JavaScript APIs remain consistent.
- Keep the future Angular client aligned with the server contract and L2 requirements; do not implement client-only authorization or safeguarding rules.
- Test narrow/mobile layouts, keyboard operation, focus, loading, empty, error, offline, and permission-denied states for UI changes.

## Commits and pull requests

Write concise imperative commit subjects, for example `auth: rotate refresh tokens` or `docs: clarify consent test setup`. A pull request should:

- Explain the user or contributor problem and the resulting behaviour
- Link related issues and L2 requirement IDs
- Include test evidence appropriate to the change
- Call out security, safeguarding, privacy, migration, and rollback implications
- Keep generated files and unrelated changes out of the diff

At least one maintainer approval is required.

## Recognition

Merged human contributions are recognized in [CONTRIBUTORS.md](CONTRIBUTORS.md) and the repository's contributors graph. Notable user-facing changes should also be added under `Unreleased` in [CHANGELOG.md](CHANGELOG.md).
