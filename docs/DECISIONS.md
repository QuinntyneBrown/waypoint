# Architecture Decisions

A running log of technical and product decisions worth recording. Each entry states the decision, why it was made, and its consequences. Newer decisions may supersede older ones; superseded entries remain for history.

## 1. Requirements are organized by subsystem and traced from acceptance tests

**Decision.** Maintain stable L1 and L2 IDs under `docs/specs`, with every acceptance test carrying a machine-readable `Traces to:` comment for the L2 requirements it verifies.

**Why.** Waypoint spans safeguarding, learning, mentoring, administration, communication, and reporting. Stable IDs keep behaviour reviewable as implementation proceeds and make coverage gaps visible.

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

**Why.** Waypoint handles youth, guardian, and program information. A hidden button or client-side route guard can be bypassed and cannot be the source of truth for access or consent.

**Consequences.** Permission and safeguarding changes require backend acceptance tests. Future clients must render server state faithfully and handle forbidden, gated, and redacted responses.

## 6. Static mocks remain the interaction reference during client implementation

**Decision.** Keep the framework-free HTML screens under `docs/mocks` as a stable review artifact while the Angular application is built separately under `frontend`.

**Why.** Stakeholders can review the complete role-based experience without a build or backend, and implementation can compare against one shared visual and interaction contract.

**Consequences.** Mock data is fictional and mock behaviour is not production logic. Shared mock changes follow `docs/contract.md`; Angular implementation must still satisfy the API contract, accessibility expectations, and L2 requirements.

## 7. The Angular workspace is scaffolded before the application vertical slice

**Decision.** Commit the Angular 21 workspace and dependency lockfile before generating the first application target.

**Why.** Toolchain and package versions can be reviewed independently while backend acceptance slices establish the initial server contract.

**Consequences.** `ng build`, `ng serve`, and `ng test` were not valid repository gates until a project was defined in `angular.json`. **Superseded by #9:** the `portal` and `public-site` applications now exist, so those commands (plus the Playwright e2e suite) are enforced by the frontend CI workflow.

## 8. Real participant data is prohibited during early development

**Decision.** Use synthetic data in code, tests, mock screens, screenshots, issues, and logs until production security, privacy, accessibility, and safeguarding readiness is explicitly approved.

**Why.** The product domain includes minors, guardians, consent, attendance, communications, and progress records. Early-stage engineering environments do not yet have the necessary operational controls.

**Consequences.** Contributors must redact sensitive evidence and follow `SECURITY.md`. A green test suite does not authorize production deployment or collection of real participant information.

## 9. Two-app Angular frontend on a same-origin proxy with in-memory access tokens

**Decision.** Build the frontend as two applications — `portal` (authenticated SPA) and `public-site` (SSR/prerendered) — over shared libraries consumed via `@waypoint/*` TypeScript path aliases pointing at library **source** (the library layering is in #16; `npm run build` also runs `packages:build` and `packages:check`). E2E and dev run both apps behind an Angular dev-server proxy that forwards `/api` to the backend, so the browser makes same-origin requests and the 4200-only CORS policy never bites. The access token lives in memory only; the rotating refresh token + user snapshot persist in `sessionStorage` (or `localStorage` when "remember me" is checked), and a boot initializer re-mints the access token via refresh.

**Why.** The public site needs SSR/SEO and must not carry auth/shell code, while all signed-in roles share one session, shell, and design system — so one authenticated app with lazy role areas plus one public app fits better than one-app or one-app-per-role. Source-aliased libraries keep the ATDD loop fast (no lib rebuild during development). A same-origin proxy matches production deployment and removes CORS from the test path. Keeping the access token out of persistent storage limits its exposure; XSS is the threat model, so Angular's default sanitization is never bypassed on user content.

**Consequences.** The original Tier A/Tier B delivery split is complete: Testing-only HTTP fixtures provision Admin/Mentor/Guardian/Volunteer identities for browser acceptance while production exposes no equivalent role-granting seam. Every acceptance slice is traced to an L2 requirement and driven through the real app against the real API. The client renders forbidden/gated/redacted responses faithfully but is never the access authority (see #5).

## 10. Transactional email and scheduled notifications use leased database queues

**Decision.** Application handlers enqueue transactional email in an encrypted database-backed outbox in the same relational transaction as the business and audit changes. Each account-directed envelope records its recipient account independently from any participant whose data appears in the content. SQL Server transaction-owned, per-user application locks serialize account correction/removal with linked enqueue, notification production, and SMTP delivery; producers and workers re-read authoritative account status and the current recipient address after taking the lock. A hosted worker claims bounded batches with expiring leases, sends through configurable SMTP, retries with backoff and stable idempotency keys, and removes sent/dead envelopes after the configured retention period. A separate leased worker dispatches due scheduled notifications; reading a notification feed never performs background work. Realtime SignalR callbacks are buffered inside atomic requests, discarded on rollback, and published best-effort only after a successful commit. Application URLs are configured centrally so invitation, password-reset, consent, privacy, and notification emails contain absolute links.

**Why.** Request handlers must not depend on SMTP availability, feed traffic must not be required for reminders, and multiple API instances must not deliver the same scheduled row concurrently. Email addresses and bodies are participant data and need the same at-rest protection as other sensitive content.

**Consequences.** Production startup requires HTTPS application URLs and secure SMTP configuration. A removal-completion message is the only explicit subject-linked exception allowed after anonymization; its envelope is purged after delivery. Testing replaces email transport with a capture and invokes processors explicitly where deterministic delivery is required. Relational rollback, correction/removal contention in both directions, post-commit realtime behavior, queue retry/lease/retention, and SMTP acceptance boundaries are integration-tested, but a deployment must still perform a live handshake test with its chosen SMTP provider.

## 11. Sensitive participant content uses application-level authenticated encryption

**Decision.** Apply randomized AES-GCM conversion to sensitive free text, contact details, notification payloads, audit context, and uploaded file names/content before SQL persistence. The relational database lifecycle runs a generic, key-aware legacy backfill after schema migrations and before serving; it authenticates existing ciphertext, rewrites plaintext provider values, and fails closed on a wrong key or tampering. Append-only audit details and cross-account notification copy are content-free; the safeguarding-required audit source is limited to a canonical channel, coarse device class, and masked peer-network location, never a raw IP address or user-agent string. Transactional email envelopes carry separate recipient-account and nullable data-subject links so removal can purge both account mail and copies sent to staff or guardians, while access exports expose only safe delivery metadata. Public submissions and application perspectives are included in privacy workflows only through explicit account IDs established by an authenticated author or by an administrator after separate ownership verification; submitted email addresses never establish ownership. Competing administrator claims use conditional database updates, so one account wins rather than last-writer-wins. Erasure is perspective-aware: removing a guardian clears guardian contact data without deleting the child's application, and removing a staff actor pseudonymizes that actor without erasing the youth's safeguarding or learning record.

**Why.** Database access, backups, and queued cross-recipient copies must not reveal safeguarding, learning, consent, support, or uploaded participant content. Encryption alone is not erasure, so duplicate prose and raw client identifiers must not be written into append-only ledgers, and subject-related delivery copies need an explicit lifecycle link independent of their recipient address. Email equality is not reliable evidence that two people own the same record; exact relational links make export, correction, and erasure both safer and bounded. Coarse device and network context preserves the accountability required by `L2-CONSENT-003` while minimizing the source retained for that purpose.

**Consequences.** Production requires a stable secret encryption key and an operational key-rotation plan. Encrypted randomized columns cannot be filtered by freshly encrypted SQL parameters, so privacy collection queries use indexed ownership IDs; normalized invitation addresses remain separately indexed because they are operational identifiers rather than ownership evidence. Existing approved applications backfill a guardian link only from the authoritative `CreatedYouthUserId -> Users.GuardianUserId` relationship. Anonymous records remain unlinked until separately verified, and the access export explains that boundary. Fresh/upgrade migration, conditional-claim contention, plaintext-backfill, round-trip, tamper, correction, export-redaction, and removal-sentinel behavior are integration-tested. Environment owners still need encrypted backups, least-privilege database access, query-plan/data-volume validation, and recovery exercises.

## 12. The product is named Waypoint

**Decision.** Rename the product, repository, solution, projects, packages, CLI, design-system prefix, and all documentation from "Word Up" to **Waypoint**. Every spelling collapses to one word: `Waypoint` in prose, namespaces, and types (was `WordUp`); `waypoint` in file names, URLs, and email domains (was `wordup` and `word-up`); `WAYPOINT` in environment variables (was `WORDUP` and `WORD_UP`). The frontend prefix `wu` becomes `w` (`w-` CSS classes and selectors, `--w-*` tokens, `@waypoint/api`, `@waypoint/auth`, `@waypoint/components`, `W*` classes, `W_*` constants; `@wu/*` and `wu-ui` before). The dotnet tool command `wu` becomes `way`. The consent-state response header becomes `X-Waypoint-State` (was `X-WordUp-State`), and the personal-data export file becomes `waypoint-personal-data-*.json`.

**Why.** "Word Up" said nothing about what the software does and carried an unrelated cultural reference. "Waypoint" describes the product: milestones, progression, and the journey a young person takes through the program. A single-word name also removes the six spelling variants that had accumulated.

**Consequences.** The GitHub repository is `QuinntyneBrown/waypoint`; GitHub redirects the old `word-up` URLs. Existing development databases named `WordUp*` and tokens issued with the `WordUp` JWT issuer are not migrated; recreate them. The rendered C4 diagrams under `docs/detailed-designs` were re-rendered with the new labels.

## 13. Donations are monthly tiers handed off to an external provider

**Decision.** The public site offers recurring monthly donation tiers of $20, $50 and $100 and hands the visitor off to an external PCI-compliant provider; Waypoint never collects or stores card data. The mock, the spec (`L2-PUB-003` AC3) and the code agree on these tiers.

**Why.** The tiers disagreed between the mock ($25/$60/$150) and the code ($20/$50/$100). The code and its tests are the shipped behavior, so the mock follows it.

**Consequences.** The provider has not been chosen: `donate.ts` still hands off to the placeholder `https://donate.example-pci-provider.org/waypoint`. The product owner must supply the real provider and URL before production; that is a one-line change to `providerUrl`.

## 14. Spec Appendix A values are confirmed as built

**Decision.** The ten values in `docs/specs/README.md` Appendix A are accepted as implemented: cohort status from one source (A-1), youth ages 12-18 (A-2), 14-day invitation expiry (A-3), passwords of 8+ characters with a letter and a number (A-4), the six system roles (A-5), best passing quiz attempt (A-6), single-source progress counts (A-7), no federated sign-in (A-8), external-provider monthly donations (A-9, DECISIONS #13) and System/Light/Dark themes (A-10).

**Why.** Implementation is complete and every value is covered by tests; leaving the appendix titled "to Confirm" implied unfinished sign-off.

**Consequences.** Changing any value now is a normal requirement change (L2, design, mock, tests), not a pending confirmation.

## 15. Test rules match how the suite actually runs

**Decision.** (1) Playwright acceptance tests run against the real API (DECISIONS #9); only Vitest unit tests are backend-free. (2) The no-selectors-in-tests rule applies to new and changed specs; legacy specs migrate to page objects when next touched. (3) The requirement traceability gate (`scripts/check-traceability.mjs --strict`) is the single allowed exception to the no-architecture-tests rule. It stays in both `backend.yml` and `frontend.yml` because each workflow's path filter would otherwise miss changes to the other side's tests. `--strict-ac` stays opt-in until legacy tests carry AC annotations.

**Why.** `AGENTS.md` contradicted the Playwright config, the CI workflows, DECISIONS #9 and the traceability gate that CI already enforces.

**Consequences.** Rewriting the e2e suite to route interception, or moving all 206 inline selectors at once, is not planned.

## 16. The frontend workspace is layered into apps, feature libraries, and shared libraries

**Decision.** Ten `@waypoint/*` libraries (`frontend/tsconfig.json`): `api` and `components` have no workspace dependencies; `auth`, `core`, `account` and the role libraries `youth`, `mentor`, `admin`, `guardian`, `volunteer` hold the portal's pages and services. Feature libraries may not depend on one another's role libraries; `packages:check` enforces this. Lazy-loaded code (auth pages, realtime, attendance queue) lives in secondary entry points. `npm run build` runs `packages:build`, the app builds, then `packages:check`.

**Why.** One portal app with every role's pages in it was hard to navigate and let roles import each other's code. Secondary entry points keep the initial bundle at its previous size.

**Consequences.** Supersedes the three-library description in #9. Adding a library means updating `tsconfig.json` paths, `packages:build` and the package check.

## 17. The design system is extracted from the mocks

**Decision.** `docs/design-system/` holds tokens (light and dark, DTCG export, checked contrast pairs), token-only component CSS, foundation, component and pattern pages. All mocks consume the design-system CSS; the old per-mock stylesheets and `styleguide.html` were deleted.

**Why.** Mocks carried divergent page-local variables. One token source makes mocks and the frontend converge (see DECISIONS #6).

**Consequences.** Design-system and mock work is design-artifact work: no tests are written for it. Whether the Angular components consume these tokens is tracked separately.

## 18. The frontend uses ESLint, Storybook and Husky

**Decision.** `angular-eslint` (`npm run lint`), Storybook for the `components` library (`npm run storybook`, `build-storybook`) and a Husky pre-commit hook running `lint-staged` (eslint --fix, prettier).

**Why.** Catch template and accessibility mistakes before review, and give components an isolated place to be exercised.

**Consequences.** Lint and Storybook are not CI gates yet. Which of Storybook and `docs/design-system` is the component reference is open.

## 19. User guides were removed

**Decision.** The `user-guides/` tree, its GitHub Actions workflow, the vendored-stylesheet gate script, and the references to them were deleted.

**Why.** The workflow and gate failed on every push once the tree was gone.

**Consequences.** There is no hosted user guide. The narrated journey videos and `docs/demo-plan.md` are the walkthrough material.

## 20. Acceptance tests are the main coverage strategy; unit tests are the exception

**Decision.** Behaviour is proven by HTTP integration tests (backend) and Playwright acceptance tests (frontend), each traced to an L2 requirement. Unit tests are added where logic is isolated and cheap to test (validators, pure helpers). Libraries without specs (`youth`, `mentor`, `guardian`, `volunteer`) get a `test` target only when they gain one.

**Why.** The ATDD process in `AGENTS.md` drives every slice from an acceptance test; unit tests written afterward would duplicate that coverage and are barred by the incremental-implementation rule.

**Consequences.** Low unit-test counts are not a gap by themselves. Add a unit test when a rule has many branches that acceptance tests cannot reach economically.
