# Frontend Implementation Plan — Angular Workspace

Delivery plan for building the Word Up frontend under the repository's mandatory ATDD loop (see `CLAUDE.md`): every slice starts with a failing Playwright acceptance test traced to an L2 requirement, driven through the real Angular app against the real API. Workspace shape per [frontend-structure.md](frontend-structure.md) (`portal` + `public-site` apps, `wu-ui`/`wu-api`/`wu-auth` libraries). Companion docs: [backend-implementation-plan.md](backend-implementation-plan.md), [implementation-plan.md](implementation-plan.md), [`specs/`](specs/README.md).

## Context and scope decisions

`frontend/` is a bare Angular 21 workspace (`projects: {}` — no runnable target). The backend is substantially ahead of `docs/ARCHITECTURE.md`: the previously inventoried surface was 84 endpoints (auth, invitations, password reset, onboarding, intake, consent, audit, people/cohorts/org-settings, curriculum authoring, mentor suite, youth learning), and commit `7e91cec` added announcements, attendance, messaging, notifications, notification & display preferences, profile + change-password, reports, RSVPs, sessions, support, roster import/export, an access-state query (`GET /api/me/access`), a maintenance middleware, and a non-Production `GET /api/_diag/throw` diagnostic. That delta has been inventoried at endpoint level (auth gating, request/response shapes, validation) and is reflected below. Verified as still true after the delta: **no SignalR hub** (notifications are polled DB rows), **no seeding or role-granting path** (self-service register still yields only an active Youth), and no CORS/rate-limit/appsettings changes.

**Product-owner decisions governing this plan:**

1. **Scope = surfaces provable end-to-end against today's API** — not a foundation-only scaffold, and no mock-data build-out of screens whose backend doesn't exist.
2. **Frontend only — no backend changes.** Screens blocked by a missing endpoint or unreachable role are deferred and listed with their blocker, not unblocked by new backend work.

**Hard consequence (verified in code):** the backend has no data seeder and no EF migrations; its own integration tests mint admin/mentor users by writing to the database via the in-process DbContext (`backend/tests/WordUp.IntegrationTests/IntegrationTestBase.cs`), which an external e2e harness cannot reach. The only self-service identities are anonymous visitors and `POST /api/auth/register` → an active **Youth**. That splits the work into:

- **Tier A (this plan):** workspace scaffolding, design-system port, public site + intake, auth, youth onboarding, access/consent gate, state screens, and the verified self-scoped `[Authorize]` surfaces from the delta — profile + change-password, notification & display preferences, support requests, youth announcement/notification feeds — plus a11y/responsive hardening.
- **Tier B (deferred, inventoried at the end):** everything requiring an Admin, Mentor, Guardian, or Volunteer account, or content those roles author.

## Key API facts the implementation relies on (verified)

- Tokens are returned in the JSON body only — `AuthResult { accessToken, expiresAt, refreshToken, userId, email, displayName, role }`; no cookies. Refresh = `POST /api/auth/refresh` (rotation; reuse revokes the token family → 401). Access token ~60 min; refresh 30 days.
- Lockout: 5 failed sign-ins → 423 for 15 min. Uniform error envelope `{ title, errors? }` (400/401/403/404/409/410/423). Sign-in failure copy identical for unknown email vs wrong password.
- CORS allows exactly `http://localhost:4200`; rate limiting 20/min on `/api/auth*` unless `ASPNETCORE_ENVIRONMENT=Testing`; DB defaults to EF InMemory (process-private); Swagger always on at `/swagger/v1/swagger.json` (used as a manual model cross-check each slice).
- API launch: `dotnet run --project backend/src/WordUp.Api` (no launchSettings; pass `--urls` explicitly).
- New in `7e91cec` (all verified): self-scoped `PUT /api/me/profile`; `POST /api/me/change-password` (wrong current password → 403; success revokes all other sessions); `GET/PUT /api/me/notification-preferences` (upsert per event key; defaults in-app ON); `GET/PUT /api/me/display-preferences` (`textScalePercent` 80–200, contrast/motion/underline/reading-font); `POST /api/support` (category/description/reply-to; authenticated only); `GET /api/announcements` + `read`/`read-all` (visibility by audience + role targeting; only *sent* announcements); `GET /api/notifications` + `read-all` (polled DB rows); `GET /api/me/access` (Youth) → `{ hasCohort, consentRequired, participationAllowed }`. Maintenance mode: config flag `Maintenance:Enabled` → **503 + `Retry-After: 120`** on every path except `/health`. Staff-gated delta endpoints (sessions, attendance, RSVP list, reports, import/export, announcement compose/stats, mentor broadcast) are Tier B; CSV exports return JSON `{ fileName, content }`, not a file download.

## E2E acceptance harness

- **Topology:** API on `http://127.0.0.1:5211`, `portal` on `:4200`, `public-site` on `:4201`. Both dev servers proxy `/api` → 5211 via `frontend/proxy.conf.json` wired into each `serve` target; `wu-api` uses a relative base URL behind an `API_BASE_URL` injection token (default `''`). The browser sees same-origin requests, so the 4200-only CORS policy never bites (including public-site's intake POST from 4201), and it matches a same-origin production deployment.
- **Orchestration:** `frontend/playwright.config.ts` with a `webServer` array:
  1. `dotnet run --project ../backend/src/WordUp.Api --no-build --urls http://127.0.0.1:5211` — env `ASPNETCORE_ENVIRONMENT=Testing`, `ConnectionStrings__Default=InMemory`, an e2e-only `Jwt__SigningKey`; health-checked on `/health`; CI runs `dotnet build backend/WordUp.sln` first so `--no-build` boots fast.
  2. `npx ng serve portal --port 4200`
  3. `npx ng serve public-site --port 4201`
  Playwright projects `portal` / `public-site` with matching `baseURL` and `testDir`. `reuseExistingServer: !process.env.CI` for fast local iteration (also sidesteps Windows process-tree kill orphaning Kestrel). npm scripts: `api:build`, `api`, `e2e`, `e2e:ui`.
- **Seeding = API-creatable state only:** a fixture `registerYouth(request, tag)` calls `POST /api/auth/register` with `e2e-<tag>-<uuid>@wordup.test` and returns credentials + `AuthResult`. No register UI is built (provisioning is invite-only per product; the endpoint is used purely as a data channel through Playwright's `request` context). The intake e2e creates its own application via the anonymous `POST /api/applications`.
- **Isolation:** one API process and one shared InMemory DB per run; **unique-data-per-test, fully parallel** — tests only assert on data they created, never on global lists or counts. No restart-per-fixture (serializes the suite for no isolation gain). When Tier B ever lands, each global-singleton resource (e.g. org settings) gets exactly one `test.describe.configure({ mode: 'serial' })` spec file.
- **Layout & tracing:**
  ```
  frontend/e2e/
  ├── support/          # registerYouth(), signIn(page, creds), api helpers — no trace lines
  ├── portal/           # auth/ onboarding/ youth/ states/ prefs/  — kebab-case *.spec.ts
  └── public-site/      # apply.spec.ts landing.spec.ts donate.spec.ts
  ```
  One `// Traces to: L2-<AREA>-NNN` comment inside each acceptance test. `scripts/check-traceability.mjs` already scans `frontend/e2e/**` for exactly this format — the script needs no changes, and the traceability gate stays e2e-only by convention (unit specs live outside the scanned tree). `frontend/e2e/tsconfig.json` keeps e2e out of app builds; `@playwright/test` as a devDependency.
- **Password-reset confirm token:** the raw token exists only in `LoggingEmailSender` output on API stdout. Capturing stdout is possible without backend changes but couples the suite to an incidental log format — **not built**. Reset e2e covers the request flow (identical confirmation for existing vs unknown email, masked email, resend), all client-side confirm-form behavior (mismatch-blocked submit, policy feedback, show/hide toggles), and garbage-token → **410 Gone** state. The happy-path confirm (AC3) is deferred; the stdout-capture trick is documented as an optional, explicitly-fragile follow-up slice.

## `wu-auth` session design

- **Storage split:** access token in memory only (signal store — never persisted; reloads re-mint via refresh). Persisted state (`wu.session.v1`) = refresh token + user snapshot, in `sessionStorage` when remember-me is unchecked (the default per L2-AUTH-001 AC4) or `localStorage` when checked (with shared-device warning copy). A `SessionPersistence` strategy interface isolates the stricter reading of AC4 (memory-only when unchecked) to one file — decided at the failing test.
- **Boot restore:** `provideAppInitializer` reads storage; a stored refresh token → `POST /api/auth/refresh` (write the rotated token back); 401 clears silently and guards route to sign-in.
- **Refresh scheduling:** compute time-to-expiry once at response receipt; `setTimeout` at expiry − 2 min (floor 5 s); never re-derive from wall clock later (server tolerates 30 s skew). Background-tab timer throttling is absorbed by the interceptor backstop.
- **Functional interceptor:** attaches `Authorization: Bearer`; on 401 (except `/api/auth/refresh` itself) runs a **single-flight** refresh — concurrent 401'd requests await one shared promise, then retry once. Refresh failure or family revocation (L2-AUTH-012 AC2) → forced teardown → `/session-expired`.
- **Two-grade teardown:** explicit sign-out = best-effort `POST /api/auth/sign-out`, purge all `wu.*` keys, hard reload to sign-in (L2-AUTH-007 AC1). Forced expiry = purge session but preserve `wu.draft.<userId>.*` so drafts recover after re-auth (L2-PLATFORM-010 AC1).
- **Role-home routing:** all five roles mapped (`/youth`, `/mentor`, `/admin`, `/guardian`, `/volunteer`); only the Youth path is e2e-provable in Tier A — the rest are unit-tested now. Youth entry resolver chain: `GET /api/consent/status` → blocked → `/consent-required`; `GET /api/onboarding` → incomplete → `/onboarding` (this is what makes onboarding resumable, L2-AUTH-011 AC4). Sign-in error map: 400 → generic enumeration-safe copy; 423 → locked screen (duration + reset/contact-mentor paths); 403 → account-deactivated state.
- **Deterministic refresh e2e (no 60-min waits):** sign in through the UI, then via the API request context present the captured refresh token twice — reuse detection revokes the family — then navigate; the app's next refresh 401s and the test asserts forced sign-out to `/session-expired`. Traces L2-AUTH-012 with a register-created Youth.
- Guards and role directives stay presentational only; the API response is the authority (DECISIONS.md #5).

## Milestones

Each milestone is a stack of red→green ATDD slices; commit only with the full suite green. Sizes: S ≈ 1–2 slices, M ≈ 3–6, L ≈ 7–12.

```
M0 harness ── M1 design system ──┬── M2 auth ──┬── M3 password reset
                                 │             ├── M4 onboarding + no-cohort
                                 │             └── M5 consent gate (youth)
                                 └── M6 public-site + intake   (parallel to M2–M5)
M7 settings & preferences                      M8 states + a11y/responsive hardening
```

| # | Milestone | Size | Acceptance traces | Mock screens |
|---|---|---|---|---|
| M0 | Scaffold: `ng g application portal --prefix=wu --style=css` (defaults: standalone, zoneless, vitest) + `ng g library wu-ui / wu-api / wu-auth --prefix=wu`; `@wu/*` path aliases; prettier pass over generated code; Playwright + proxy + webServer config; smoke e2e (portal boots, `/health` via proxy); `.github/workflows/frontend.yml` | S–M | smoke untraced (gate allows) | — |
| M1 | Design system port: `_core.css` tokens/styles into `wu-ui`; fonts decoded to real `.woff2` in `public/fonts/` (see Risks #1); icon registry (~100 icons from the `wordup.js` `ICONS` map); auth-first primitives (authcard, field/input/error, buttons, alert); toast + modal services (focus trap, Escape, return-focus); shell components from the per-role NAV config; avatar (initials + 8-color hash palette), badge, card, skeleton, empty — grown lazily per consuming slice, not big-bang | L | proven via consuming slices; vitest unit specs for modal/toast/custom controls | `styleguide.html`, `_template-*.html`, `shared/` |
| M2 | Auth slice: sign-in (uniform failure copy; remember-me default-off + warning when checked), sign-out full teardown, silent refresh + family-revocation forced sign-out, lockout screen (deterministic: 5 fails, Testing env disables rate limiting), session-expired with draft preservation, 404 state, role-home routing | M | L2-AUTH-001, -006, -007, -012; L2-PLATFORM-007, -010 (AC1) | `auth/login`, `states/404`, `states/session-expired` |
| M3 | Password reset: request flow (identical confirmation, masked email, resend); confirm-form client validation (mismatch block, policy feedback, show/hide toggles); invalid token → 410 state; happy-path confirm deferred (harness section) | S–M | L2-AUTH-005 (AC1/2/4/5), L2-AUTH-008 client ACs | `auth/` reset screens |
| M4 | Onboarding wizard: name/pronouns/photo → read-only cohort step (register-created youth are unplaced → also proves the **no-cohort** state) → notification-defaults step (defaults ON/ON/OFF, persisted via `PUT /api/me/notification-preferences` — verified self-scoped, any role) → "Skip for now" lands home, resumable via resolver | S–M | L2-AUTH-011; L2-PLATFORM-011 (AC3) | `youth/onboarding*`, `states/no-cohort` |
| M5 | Access gate: youth-entry resolver on `GET /api/me/access` → `{ hasCohort, consentRequired, participationAllowed }`. **Deterministic in Tier A** (verified: a register-created youth has no cohort and no consent record → `false / true / false`), so both the **consent-required** and **no-cohort** gate states are e2e-provable — no discovery test needed. Consent-required screen ships with guardian-notify copy only (no notify endpoint exists). The unblock transition (consent recorded → youth flows through) needs a Guardian → Tier B | S | L2-CONSENT-002 (AC1 client), L2-PLATFORM-011 (AC1/AC3 partial) | `states/consent-required`, `states/no-cohort` |
| M6 | Public-site (`ng g application public-site --prefix=wu --ssr`) + intake: landing (4-step journey, impact), programs (static track content), apply form (who-is-filling selector; conditional guardian fields when age < 18; 12–18 age-gate messaging; field-level errors blocking submit; confirmation + "no account yet"), donate handoff (tier select → external PCI provider, no card data), privacy, marketing nav/footer, working CTAs on every page. (Verified: the new `POST /api/support` is `[Authorize]`-gated, so the anonymous public contact/volunteer forms stay deferred — see Tier B) | M | L2-INTAKE-001; L2-PUB-001, -002 (AC1/3), -003 (AC3); L2-NFR-009 | `public/*` |
| M7 | Settings & preferences — new backend from `7e91cec`, all verified self-scoped `[Authorize]` (any role): profile settings with read-only placement fields + photo (`PUT /api/me/profile`); change-password e2e (wrong current → 403 copy; success → other sessions revoked, old refresh dead, new password signs in); notification preferences round-trip (`GET/PUT /api/me/notification-preferences`); display & accessibility preferences with **server persistence** (`GET/PUT /api/me/display-preferences`, scale 80–200 validation surfaced inline; text-size live preview, high-contrast / reduced-motion / underline-links / reading-font as root-level token hooks); help center + support submission (`POST /api/support` → acknowledgement with id); youth announcements/notifications feeds (verified any-authenticated reads: empty-state + read-all e2e; non-empty feeds need staff-composed content → Tier B) | M | L2-PLATFORM-002, -003, -004, -005 (+ the youth-reachable ENGAGE feed subset) | `shared/settings-*`, `shared/help`, `shared/support`, `youth/announcements`* |
| M8 | States + hardening: offline banner/state (connectivity detection, cached reads usable, L2-PLATFORM-012 AC1/AC3); 500 state with retry — the app's 500 handling is e2e'd by Playwright route-fulfilling one API call with the real error envelope (`{ title: "An unexpected error occurred." }`) — **the one deliberately simulated seam, flagged in the spec file** (the server-side 500 contract itself is backend-tested via the new `GET /api/_diag/throw`); 503 maintenance — verified: config flag `Maintenance:Enabled` → 503 + `Retry-After: 120`, `/health` exempt — e2e via a small dedicated Playwright project booting a second API instance (port 5212, flag on) + a portal dev server proxying to it, or component-test the interceptor mapping if that proves too heavy; 403 screen (screen ships; a *real* 403 e2e needs a role mix → Tier B); a11y/responsive passes via a `portal-xs` mobile-viewport Playwright project (keyboard operability, focus trap, XS table→card reflow) | M | L2-PLATFORM-006 (part), -008, -009, -012 (AC1/3); L2-NFR-009, -010 | `states/*`, all prior screens |

Rationale: M0–M2 puts a real registered user through sign-in/refresh/sign-out against the real API within three milestones — harness, proxy, tokens, and shell proven end-to-end before feature breadth. M6 needs no auth and runs parallel to M2–M5.

## Tier B — deferred (blocker named; no backend work planned)

Blocked because the role (or content only that role can author) cannot be obtained through any API-reachable path:

| Deferred surface | Blocker | Deferred traces |
|---|---|---|
| Invitation acceptance + invite-expired flow; staff invite management | Creating an invitation needs an `InviteUsers` role; raw token only in logged email; no `GET /api/invitations` | L2-AUTH-003, -004; L2-PLATFORM-011 (AC2) |
| Youth learning suite (curriculum map, lessons, reflections, quizzes, submissions, badges, progress) | Content requires an `EditCurriculum` admin to author; several youth-visible reads missing | L2-LEARN-*; L2-NFR-007 |
| Mentor suite (dashboard, roster, flags, check-ins, competencies, assign-work, grading, quiz approval) | Needs Mentor role | L2-MENTOR-* |
| Admin suite (people, cohorts, org settings, application review, authoring, audit view, reports, import/export UI) | Needs ProgramAdministrator; also no `GET /api/applications` list. Note for that day: CSV exports arrive as JSON `{ fileName, content }` — the client materializes the download (Blob), no `text/csv` endpoint exists | L2-ADMIN-*; L2-INTAKE-002 client; L2-CURRIC-*; L2-CONSENT-003 view; L2-REPORT-* |
| Guardian consent capture/withdrawal + guardian portal | Needs Guardian role + guardian↔youth link (only via admin approval) | L2-CONSENT-001; L2-GUARDIAN-* |
| Volunteer portal; sessions/calendar UI; attendance capture + offline queue; youth RSVP | Verified gating: attendance writes need the `RecordAttendance` policy (Admin/Mentor/Volunteer; volunteers limited to Present/Absent), sessions are created by Admin/Mentor, and youth RSVP needs a staff-created session to exist. Attendance upsert is idempotent by (session, youth) — good news for the future offline queue — but there is no client-supplied idempotency key | L2-VOLUNTEER-*; L2-ENGAGE-*; L2-NFR-004 |
| Messaging UI | Verified scope: a youth may message only their assigned mentor (register-created youth have none); mentor/admin side needs those roles. Bodies stored HTML-encoded server-side | L2-ENGAGE messaging |
| Announcement compose/stats; non-empty feeds; mentor broadcast | Compose/stats need Admin/Mentor; a non-empty youth feed needs staff-composed content | L2-ENGAGE announcements |
| Public contact + volunteer forms | The new `POST /api/support` is `[Authorize]`-gated; no anonymous contact endpoint exists | L2-PUB-003 (AC1/AC2) |
| Account-deactivated e2e; real-403 e2e; reset happy-path confirm | Producing the state needs admin action / logged token (screens ship in Tier A) | L2-PLATFORM-010 (AC2), -006 (full); L2-AUTH-005 (AC3) |
| Public cohort-status list | No public cohorts endpoint (`GET /api/cohorts` is admin-gated) | L2-PUB-002 (AC2/AC4) |
| `wu-realtime` / live delivery | Verified: no hub anywhere in `backend/src` — notifications/messages are polled DB rows. Generate the library only when a hub exists | L2-NFR-005 |

**Future unblock (recorded, not planned):** the cheapest no-production-change path is a small seed CLI under test infrastructure referencing `WordUp.Infrastructure` — but EF InMemory is process-private, so that day implies e2e against real SQL (LocalDB locally; the existing SQL container in CI). The alternative (a Testing-only seed endpoint) keeps InMemory but touches production code. Decision deferred with Tier B.

## CI — `.github/workflows/frontend.yml`

Triggers: `push` + `pull_request` on `frontend/**`, `backend/src/**` (backend changes can break the e2e contract), `docs/specs/**`, `scripts/**`, and the workflow file. Two parallel ubuntu jobs; **no SQL Server container** (API runs InMemory):

- **unit:** checkout → setup-node 22 (npm cache keyed on `frontend/package-lock.json`) → `npm ci` → `npx prettier --check .` → `npx ng build portal` + `npx ng build public-site` → `npx ng test` (the `@angular/build:unit-test` builder driving vitest on jsdom — no browser install).
- **e2e:** checkout → setup-dotnet 9 + setup-node 22 → `dotnet build backend/WordUp.sln` → `npm ci` → `npx playwright install --with-deps chromium` → `npx playwright test` (the webServer array boots API + both apps) → `node scripts/check-traceability.mjs` (reverse gate: an invalid L2 id in any e2e spec fails CI) → upload `playwright-report` artifact on failure.

Chromium-only for speed; the traceability gate stays non-`--strict`, matching `backend.yml`. Until M0 lands there is deliberately no frontend CI (DECISIONS.md #7).

## Risks and gotchas

1. **Fonts vs the 500 kB initial-bundle budget:** `docs/mocks/assets/css/_fonts.css` is ~178 KB of base64 woff2 — never import it. A one-time script decodes the six `data:font/woff2` URIs into real `.woff2` files (~130 KB binary) under each app's `public/fonts/`, referenced by normal `@font-face` with `font-display: swap` — cacheable assets outside the bundle.
2. **Zoneless is the Angular 21 default** (no `zone.js` installed): signals throughout; vitest TestBed specs need `await fixture.whenStable()`; avoid libraries that assume zones. Playwright is unaffected.
3. **Vitest = jsdom:** no real layout — unit specs assert classes/ARIA/behavior, never geometry; visual/responsive assertions live in Playwright (`portal-xs`).
4. **Numeric JSON enums:** the backend registers no `JsonStringEnumConverter` — model `PublishStatus`/`QuestionType`/`BadgeCriteriaKind` as numeric TS enums in `wu-api` from the first model (competency `level` is the lone string-name exception). Getting this wrong yields 400s masquerading as validation bugs.
5. **`expiresAt` clock skew:** compute time-to-expiry once at receipt; refresh 2 min early; the interceptor single-flight retry is the backstop.
6. **Prettier drift:** generated code ≠ `.prettierrc` (printWidth 100, singleQuote) — `npx prettier --write .` after every `ng generate`; add `format`/`format:check` scripts; CI checks.
7. **Windows dev loop:** `reuseExistingServer` + a long-lived `npm run api` terminal; pinned ports 5211/4200/4201; expect a one-time Kestrel firewall prompt; `taskkill /f /im WordUp.Api.exe` clears stragglers.
8. **Shared InMemory DB + parallel workers:** unique-data-per-test is load-bearing — never assert global lists/counts.
9. **Traceability reverse gate hard-fails invalid ids:** copy ids from `docs/specs/**` headings — numbering has gaps (e.g. there is no `L2-AUTH-002`).
10. **Gate semantics are pinned by the backend's own tests:** `GET /api/me/access` returns `hasCohort=false, consentRequired=true, participationAllowed=false` for a register-created youth (AccessGateTests) — the M5 e2e builds on that verified behavior, not an assumption.
11. **Token storage is JS-readable — XSS is the threat model:** Angular default sanitization only; never `bypassSecurityTrust*` on user content (L2-NFR-001 AC2); no telemetry may capture password fields (L2-AUTH-008 AC3 — record in the auth slice).
12. **SSR public-site:** keep pages free of SSR-time API calls; verify the dev-server proxy behaves under SSR during the M0/M6 smoke before building on it.
13. **Swagger as contract check:** `/swagger/v1/swagger.json` is always on — cross-check hand-written `wu-api` models against it each slice (manual reference; codegen not used).

## Verification

Per slice: acceptance e2e written first and **seen to fail for the right reason** → minimum implementation → `ng test` (all projects) + `npx playwright test` + `dotnet test backend/WordUp.sln` (must remain green, untouched) + `node scripts/check-traceability.mjs` PASS → commit. On landing milestones: update `CHANGELOG.md`, the frontend section of `docs/ARCHITECTURE.md`, and add a `DECISIONS.md` entry for the two-app/proxy/token-storage choices.
