# Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project intends to use [Semantic Versioning](https://semver.org/spec/v2.0.0.html) when versioned releases begin.

## [Unreleased]

### Added

- .NET 9 Clean Architecture backend with API, Application, Domain, and Infrastructure projects
- JWT authentication, rotating refresh tokens, account lockout, password recovery, invitations, and role policies
- Youth application intake, onboarding, guardian consent, audit logging, and their acceptance tests
- EF Core persistence with in-memory development and SQL Server integration-test providers
- Angular 21 workspace foundations for the production web clients
- Angular frontend (Tier A): `portal` (authenticated SPA) and `public-site` (SSR/prerendered marketing + anonymous intake) apps over `wu-ui`/`wu-api`/`wu-auth` libraries — design-system port (tokens, fonts, 93 icons, toast, shell), sign-in/refresh/sign-out/lockout/session-expired, password reset, youth onboarding + no-cohort, consent gate, application intake, settings & preferences (profile, notifications, display accessibility, support), youth feeds, and offline/500/503/403 states
- Complete role portal routing for youth, mentor, guardian, volunteer, and program-administrator journeys, including the administrator dashboard and users/roles workspace
- End-to-end invitation management and acceptance: scoped single/bulk issue, pending-list filters, resend/revoke, token context, expired-link replacement requests, and onboarding hand-off
- Role-tailored searchable help center plus self-service and administrator data-rights request workflows
- Configurable SMTP delivery through an encrypted durable email outbox, absolute application links, and independent background workers for email and scheduled notifications
- Authenticated AES-GCM protection and legacy-plaintext backfill for sensitive participant fields and uploaded files, with subject-linked delivery erasure, comprehensive redacted access exports, and data-minimized device/network audit sources
- SQL-backed fixed-arrival-rate API load gates (50 distinct cohort-scoped users / 20 RPS), a throttled primary-Youth-screen budget, XS–XL responsive checks, WCAG 2.1 AA axe checks, and a route/navigation manifest gate
- Stable .NET 9 SDK selection and production fail-fast validation for database, HTTPS application URLs, secrets, and SMTP configuration
- Playwright e2e acceptance harness (real API + both apps behind a dev-server proxy) with a mobile-viewport project, plus a frontend CI workflow
- Product requirements covering 14 subsystems and a requirement traceability gate
- Roughly 115 FaithTech-styled HTML design mocks under `docs/mocks`
- Backend CI and project community-health documentation

The repository has not published a versioned release. Earlier development history is available in the [commit log](https://github.com/QuinntyneBrown/waypoint/commits/main/).
