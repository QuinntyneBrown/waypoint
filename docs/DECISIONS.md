# Architecture Decisions

A running log of technical and product decisions worth recording. Each entry states the decision, why it was made, and its consequences. Newer decisions may supersede older ones; superseded entries remain for history.

## 1. Requirements are organized by subsystem and traced from acceptance tests

**Decision.** Maintain stable L1 and L2 IDs under `docs/specs`, with every acceptance test carrying a machine-readable `Traces to:` comment for the L2 requirements it verifies.

**Why.** Word Up spans safeguarding, learning, mentoring, administration, communication, and reporting. Stable IDs keep behaviour reviewable as implementation proceeds and make coverage gaps visible.

**Consequences.** IDs are never reused. Behaviour changes begin in the owning requirement, and CI runs `scripts/check-traceability.mjs` to reject invalid test references. Valid references prove trace integrity, not complete coverage.

## 2. .NET 9 with Clean Architecture and MediatR / CQS

**Decision.** Build the backend as Domain, Application, Infrastructure, and Api projects on .NET 9, with MediatR commands and queries and thin controllers.

**Why.** The layering keeps business and safeguarding rules separate from HTTP and persistence concerns, while making use cases testable through a consistent dispatch pipeline.

**Consequences.** Api composes the system; Application owns use cases and abstractions; Infrastructure supplies external implementations; Domain remains framework-light. New use cases should follow the established command/query, handler, validator, and request/result structure.

## 3. EF Core supports in-memory development and SQL Server verification

**Decision.** Use EF Core behind `IAppDbContext`. Select the in-memory provider when the default connection string is `InMemory`, and SQL Server when a real connection string is supplied.

**Why.** The in-memory provider keeps early local API work easy to run, while SQL Server integration tests exercise relational behaviour against the intended persistent provider.

**Consequences.** In-memory behaviour is not sufficient evidence for production persistence. Integration tests require SQL Server, and production readiness requires migrations, operational configuration, backups, and data-lifecycle controls.

## 4. Authentication is server-authoritative and token based

**Decision.** Use JWT bearer access tokens with opaque rotating refresh tokens, BCrypt password hashes, hashed invitation/reset tokens, account lockout, and uniform credential failure responses.

**Why.** The browser client needs a clear API authentication contract, while rotating and one-time tokens reduce the impact of token replay. Uniform failures limit email enumeration.

**Consequences.** Signing keys and connection strings must come from secure configuration outside local development. Reuse of a rotated refresh token revokes its family. Authentication endpoints are rate-limited outside tests.

## 5. Authorization, consent, and safeguarding rules live on the server

**Decision.** Role policies are enforced by ASP.NET Core and resource/relationship rules are enforced in application handlers. Client visibility is presentational only.

**Why.** Word Up handles youth, guardian, and program information. A hidden button or client-side route guard can be bypassed and cannot be the source of truth for access or consent.

**Consequences.** Permission and safeguarding changes require backend acceptance tests. Future clients must render server state faithfully and handle forbidden, gated, and redacted responses.

## 6. Static mocks remain the interaction reference during client implementation

**Decision.** Keep the framework-free HTML screens under `docs/mocks` as a stable review artifact while the Angular application is built separately under `frontend`.

**Why.** Stakeholders can review the complete role-based experience without a build or backend, and implementation can compare against one shared visual and interaction contract.

**Consequences.** Mock data is fictional and mock behaviour is not production logic. Shared mock changes follow `docs/contract.md`; Angular implementation must still satisfy the API contract, accessibility expectations, and L2 requirements.

## 7. The Angular workspace is scaffolded before the application vertical slice

**Decision.** Commit the Angular 21 workspace and dependency lockfile before generating the first application target.

**Why.** Toolchain and package versions can be reviewed independently while backend acceptance slices establish the initial server contract.

**Consequences.** `ng build`, `ng serve`, and `ng test` are not valid repository gates until a project is defined in `angular.json`. Documentation must not imply that a production frontend is currently runnable.

## 8. Real participant data is prohibited during early development

**Decision.** Use synthetic data in code, tests, mock screens, screenshots, issues, and logs until production security, privacy, accessibility, and safeguarding readiness is explicitly approved.

**Why.** The product domain includes minors, guardians, consent, attendance, communications, and progress records. Early-stage engineering environments do not yet have the necessary operational controls.

**Consequences.** Contributors must redact sensitive evidence and follow `SECURITY.md`. A green test suite does not authorize production deployment or collection of real participant information.
