# Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project intends to use [Semantic Versioning](https://semver.org/spec/v2.0.0.html) when versioned releases begin.

## [Unreleased]

### Added

- .NET 9 Clean Architecture backend with API, Application, Domain, and Infrastructure projects
- JWT authentication, rotating refresh tokens, account lockout, password recovery, invitations, and role policies
- Youth application intake, onboarding, guardian consent, audit logging, and their acceptance tests
- EF Core persistence with in-memory development and SQL Server integration-test providers
- Angular 21 workspace for the future production web client
- Angular frontend (Tier A): `portal` (authenticated SPA) and `public-site` (SSR/prerendered marketing + anonymous intake) apps over `wu-ui`/`wu-api`/`wu-auth` libraries — design-system port (tokens, fonts, 93 icons, toast, shell), sign-in/refresh/sign-out/lockout/session-expired, password reset, youth onboarding + no-cohort, consent gate, application intake, settings & preferences (profile, notifications, display accessibility, support), youth feeds, and offline/500/503/403 states
- Playwright e2e acceptance harness (real API + both apps behind a dev-server proxy) with a mobile-viewport project, plus a frontend CI workflow
- Product requirements covering 14 subsystems and a requirement traceability gate
- Roughly 115 FaithTech-styled HTML design mocks under `docs/mocks`
- Backend CI and project community-health documentation

The repository has not published a versioned release. Earlier development history is available in the [commit log](https://github.com/QuinntyneBrown/word-up/commits/main/).
