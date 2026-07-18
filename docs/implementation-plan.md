# Word Up — Implementation Plan

## Context

Word Up is a role-aware youth leadership platform derived from the product definition in [PRD.md](PRD.md) and the acceptance-testable requirements in [`specs/`](specs/README.md). It serves youth, mentors, program administrators, guardians, volunteers, and public visitors across application intake, safeguarding, learning, mentoring, attendance, communication, and reporting.

The repository currently contains three complementary artifacts:

1. A .NET 9 backend vertical slice with SQL-backed acceptance tests.
2. A bare Angular 21 workspace awaiting its first application target.
3. Roughly 115 static HTML screens that define the intended interaction and visual language.

The product is under active development and is not approved for production use or real participant data. This plan records the route from the current foundation to a reviewable, deployable application. The detailed design for the first backend phase remains in [backend-implementation-plan.md](backend-implementation-plan.md).

## Delivery principles

- Build vertical slices from an owning L2 requirement through API, persistence, client, and acceptance test.
- Enforce authentication, authorization, consent, safeguarding, and resource scoping on the server.
- Keep every acceptance test traceable to stable requirement IDs.
- Use synthetic data until security, privacy, accessibility, and safeguarding readiness is approved.
- Treat the HTML mocks as the interaction reference and the API as the behavioural authority.
- Prefer small deployable slices over a broad collection of disconnected screens or endpoints.

## Current status

| Area                 | Status                                                                                                     | Evidence                                                                    |
| -------------------- | ---------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| Product definition   | Established                                                                                                | `docs/PRD.md`                                                               |
| Requirements         | 61 L1 and 88 L2 requirements across 14 subsystems                                                          | `docs/specs/README.md`                                                      |
| Design reference     | Complete role-based mock set                                                                               | `docs/mocks/index.html`                                                     |
| Backend foundation   | In progress; identity, invitations, intake, onboarding, password recovery, consent, and audit slices exist | `backend/src` and `backend/tests`                                           |
| Persistence          | EF Core with in-memory local mode and SQL Server integration tests                                         | `WordUp.Infrastructure` and test factory                                    |
| Frontend             | Toolchain only; no Angular application target                                                              | `frontend/angular.json`                                                     |
| Automation           | Backend build/test and traceability workflow                                                               | `.github/workflows/backend.yml`                                             |
| Production readiness | Not started                                                                                                | Security, privacy, safeguarding, accessibility, and operations gates remain |

## Phase 1 — Access, intake, and safeguarding foundation

Deliver the secure entry path for every role:

- Registration, sign-in, refresh, sign-out, lockout, and password recovery
- Individual and bulk invitations with expiry, resend, revoke, and acceptance
- Youth application submission, eligibility validation, approval, and decline
- Resumable onboarding and profile setup
- Guardian consent capture, status, withdrawal, and relationship checks
- Role policies, audit logging, redaction, health, and OpenAPI documentation
- SQL Server acceptance tests traced to the owning AUTH, INTAKE, CONSENT, NFR, and PLATFORM requirements

Exit criteria:

- Relevant backend tests pass against SQL Server.
- Traceability reports no invalid L2 references.
- Threats and deferred controls are recorded in `SECURITY.md` or `DECISIONS.md`.
- API responses do not expose credentials, raw secure tokens, or unnecessary participant data.

## Phase 2 — Program administration and operations

Deliver the administrator and staff backbone:

- People, roles, cohorts, mentor assignments, and cohort lifecycle
- Data import with validation, preview, partial-failure handling, and audit records
- Calendar, events, volunteer scheduling, attendance, and offline attendance synchronization
- Announcement and notification administration
- Curriculum, module, lesson, quiz, badge, and progression-rule authoring

Exit criteria:

- Every mutating workflow enforces role and resource scope on the server.
- Import/export and offline conflict rules are acceptance tested.
- Audit events cover the sensitive actions identified by the CONSENT and ADMIN requirements.

## Phase 3 — Youth learning and mentoring

Deliver the core program experience:

- Youth home, journey, curriculum, lessons, Bible reflections, quizzes, coding projects, and submissions
- Progress, badges, streaks, schedule, announcements, notifications, and messages
- Mentor dashboard, roster, youth detail, assignments, grading, competency, check-ins, flags, and reviews
- Shared progress calculations used consistently across youth, mentor, guardian, and reporting surfaces

Exit criteria:

- The first Angular application is generated and consumes typed API contracts.
- Responsive, keyboard, focus, reduced-motion, contrast, empty, loading, error, and offline states are tested.
- Client guards reflect server state but do not replace server authorization.
- Critical journeys have unit, integration, and end-to-end coverage.

## Phase 4 — Guardian, volunteer, public, and reporting surfaces

Complete the supporting experiences:

- Guardian progress, announcements, and consent management
- Volunteer schedule and attendance within a deliberately narrow permission boundary
- Public program information, application/referral flow, contact, privacy, and external donation handoff
- Cohort, staff, youth, attendance, and custom reports with CSV/PDF export
- Data retention, account lifecycle, deletion, and export workflows

Exit criteria:

- Cross-role data visibility is acceptance tested using least-privilege scenarios.
- Public pages meet performance, SEO, privacy, and accessibility targets.
- Exported data is scoped, auditable, and protected from formula and content injection.

## Phase 5 — Production hardening and release

- Complete independent security, privacy, safeguarding, and accessibility reviews.
- Establish secret management, durable SQL Server migrations, backups, restoration, retention, deletion, and incident response.
- Add dependency, static-analysis, frontend build/test, accessibility, and end-to-end gates to CI.
- Define deployable environments, observability, alerting, operational ownership, and rollback procedures.
- Run stakeholder acceptance against the L2 corpus and reconcile the static mocks with implemented behaviour.
- Publish a release candidate only after every required production gate has named evidence and an owner.

## Verification commands

```bash
# Backend build and tests
dotnet build backend/WordUp.sln --configuration Release
dotnet test backend/WordUp.sln --configuration Release --no-build

# Requirements integrity
node scripts/check-traceability.mjs
```

Frontend build, unit-test, accessibility, and end-to-end commands will be added when the Angular application target is created. Until then, a failing bare `ng build` is expected and must not be represented as a configured quality gate.

## Keeping this plan current

Update this document when phase scope, sequencing, or exit criteria change. Record durable technical and product choices in [DECISIONS.md](DECISIONS.md), detailed requirements in [`specs/`](specs/README.md), contributor-visible changes in [CHANGELOG.md](../CHANGELOG.md), and security limitations in [SECURITY.md](../SECURITY.md).
