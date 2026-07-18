# Gap-closure implementation plan

Working plan for closing every in-scope gap listed in `docs/not-implemented.md`.
The scope authority is `docs/specs/` (88 in-scope L2s); `docs/specs/out-of-scope.md`
is excluded (see "Explicitly excluded" in `not-implemented.md`).

## Method (ATDD — mandatory, per `CLAUDE.md`)

Every slice follows the loop:

1. Write the failing acceptance test **first**, at the real seam, traced to an L2:
   - **UI slice where the API already exists** → a Playwright E2E in `frontend/e2e/**`
     using the Page Object Model (`frontend/e2e/pages/`), tagged `// Traces to: L2-…`.
   - **Server behaviour** → an NUnit integration test in
     `backend/tests/WordUp.IntegrationTests/**` through `HttpClient`, with
     `// Traces to: L2-…` + `[Property("Req","L2-…")]`.
2. Run it; watch it fail for the right reason.
3. Minimum implementation to green.
4. Full suite green.
5. Refactor; commit.

Commit one slice at a time. Keep this file's checkboxes current as the running tracker.

## Sequencing rationale

Front-load slices whose server API already exists (fastest verified value), then
build the missing role portals, then the cross-cutting backend gaps. Resolve the two
requirement conflicts early (they gate "done").

---

## Phase A — Public site completion (server APIs already exist)

- [x] **A1 · Contact form** — name/email/topic/message, field-level validation, submit to
      `POST /api/public/contact`, acknowledgement. `L2-PUB-003` AC1.
- [x] **A2 · Volunteer sign-up form** — Get-involved form, screening-consent gate, submit to
      `POST /api/public/volunteer-signup`. `L2-PUB-003` AC2.
- [ ] **A3 · Donate submission** — post pledge to `POST /api/public/donation`, record provider
      reference/result. `L2-PUB-003` AC3.
- [ ] **A4 · Public content source + cohorts view** — Landing/Programs consume
      `GET /api/public/content`; About/current-cohorts view (neighbourhood, dates, lead mentor,
      status from the single authoritative record). `L2-PUB-002`.
- [ ] **A5 · Apply intake completeness** — persist `whoIsFilling` and cohort/track of interest
      (may need a backend command/field change first, TDD'd server-side). `L2-INTAKE-001`.

## Phase B — Cross-app navigation & error/state wiring (small, high-leverage)

- [ ] **B1 · Cross-app Sign in** — public "Sign in" opens the portal app across the deploy
      boundary; all public CTAs navigate correctly. `L2-PUB-001` AC3.
- [ ] **B2 · Signed-out help links** — account-locked "Contact your mentor" and
      account-deactivated "Contact the director" target an anonymous route, not `/support`.
      `L2-AUTH-006`, `L2-PLATFORM-010`.
- [ ] **B3 · Global API-error → state pages** — map API 403/404/500 to the 403/404/500 state
      pages; 500 preserves in-progress form data; 500 handler logs/alerts with correlation id.
      `L2-PLATFORM-006/007/008`, `L2-NFR-011`.

## Phase C — Non-youth portal shell + Volunteer portal

- [ ] **C0 · Role portal routing** — real feature routes + shell for admin/mentor/guardian/
      volunteer (today they fall through to 404 despite `roleHome()`).
- [ ] **C1 · Volunteer Today** — today's assigned session/logistics/count, no youth record data.
      `L2-VOLUNTEER-001`.
- [ ] **C2 · Volunteer schedule** — upcoming assigned sessions/status + empty state.
      `L2-VOLUNTEER-002`.
- [ ] **C3 · Volunteer attendance** — names-only roster, Present/Absent, all-present, save,
      offline owner-bound queue + sync, strict denial of other resources.
      `L2-VOLUNTEER-001`, `L2-NFR-004`.

## Phase D — Guardian portal

- [ ] **D1 · Guardian home / child progress** — read-only aggregate progress, per-module
      completion, attendance history, badges, upcoming schedule, shared check-ins. `L2-GUARDIAN-001`.
- [ ] **D2 · Guardian announcements** — targeted feed, unread, mark-all-read, states. `L2-GUARDIAN-002`.
- [ ] **D3 · Guardian consent/contact** — view e-sign record, edit contact/emergency, granular
      media consent, withdraw, email a record copy. `L2-GUARDIAN-003`, `L2-CONSENT-001`.
- [ ] **D4 · Guardian participation-consent form + consent-request delivery** — public/tokenised
      guardian consent capture; blocked youth sends secure consent link; auto-unblock.
      `L2-CONSENT-001/002`, `L2-PLATFORM-011`.

## Phase E — Youth learning & engagement

- [ ] **E1** Curriculum/track browser `L2-LEARN-001`
- [ ] **E2** Module & lesson reader (+ youth lesson-content read API) `L2-LEARN-002`
- [ ] **E3** Reflection editor (+ read/restore API, draft) `L2-LEARN-003`, `L2-NFR-007`
- [ ] **E4** Quiz intro & attempt (one-question, autosave/resume) `L2-LEARN-004`
- [ ] **E5** Quiz result/review (+ review/badge result data) `L2-LEARN-005`
- [ ] **E6** Assignment brief & submission (+ brief/read API, file upload) `L2-LEARN-006`
- [ ] **E7** Submission status/resubmission (+ enriched status data) `L2-LEARN-007`
- [ ] **E8** Coding lesson/project (missing central outcome — no covering test yet) `L2-LEARN-008`
- [ ] **E9** Journey & badges (counted criteria, milestones) `L2-LEARN-009`
- [ ] **E10** Personal progress (quiz history, attendance tabs, competencies) `L2-LEARN-010`
- [ ] **E11** Youth schedule & RSVP (youth-visible calendar, reminders) `L2-ENGAGE-005/008`
- [ ] **E12** Youth messaging (mentor thread + realtime) `L2-ENGAGE-007`, `L2-NFR-005`
- [ ] **E13** Youth home placed-state content + `/youth/messages` link fix `L2-LEARN-*`, `L2-PLATFORM-011`
- [ ] **E14** Youth notifications deep links + live events `L2-ENGAGE-008`, `L2-NFR-005`

## Phase F — Mentor portal (`L2-MENTOR-001..008`, `L2-ENGAGE-001..004/007/008`)

- [ ] **F1** Dashboard/roster/reviews preview `L2-MENTOR-001`
- [ ] **F2** Youth detail (scoped tabs) `L2-MENTOR-002`
- [ ] **F3** Submission review/grading (+ queue/detail API, outcomes) `L2-MENTOR-003`, `L2-LEARN-007`
- [ ] **F4** Quiz assessment approvals (+ pending/review queries) `L2-MENTOR-004`
- [ ] **F5** Competency tracker `L2-MENTOR-005`
- [ ] **F6** Check-ins & history (+ shared read surfaces) `L2-MENTOR-006`
- [ ] **F7** At-risk flags + automated evaluation `L2-MENTOR-007`
- [ ] **F8** Assign work (+ journey placement, message, reminders) `L2-MENTOR-008`, `L2-ENGAGE-008`
- [ ] **F9** Mentor calendar & attendance `L2-ENGAGE-001..004`
- [ ] **F10** Mentor messaging/notifications `L2-ENGAGE-007/008`, `L2-NFR-005`

## Phase G — Program Administrator portal

- [ ] **G1** Application review `L2-INTAKE-002`
- [ ] **G2** People directory `L2-ADMIN-001`
- [ ] **G3** Person detail / account lifecycle `L2-ADMIN-002`
- [ ] **G4** Cohorts & cohort detail `L2-ADMIN-003`
- [ ] **G5** Progress board (+ live, export) `L2-ADMIN-004`, `L2-REPORT-003`
- [ ] **G6** Mentor assignment / caseload `L2-ADMIN-005`
- [ ] **G7** Organization settings + term lifecycle `L2-ADMIN-006`
- [ ] **G8** Curriculum builder `L2-CURRIC-001..006`
- [ ] **G9** Calendar/session management `L2-ENGAGE-001`
- [ ] **G10** Attendance administration/history `L2-ENGAGE-002..004`
- [ ] **G11** Announcement administration `L2-ENGAGE-006`
- [ ] **G12** Reports hub + standard reports `L2-REPORT-001`
- [ ] **G13** Data export (PDF + targets) `L2-REPORT-003`
- [ ] **G14** Roster import UI `L2-REPORT-004`
- [ ] **G15** Audit log UI `L2-CONSENT-003`

## Phase H — Cross-cutting backend & platform

- [ ] **H1** Idle expiry + draft recovery framework `L2-AUTH-007`, `L2-PLATFORM-010`, `L2-NFR-007`
- [ ] **H2** Per-action consent enforcement in protected youth handlers `L2-CONSENT-002/004`
- [ ] **H3** Correct consent validity (flag/withdrawal/window/term/re-consent) `L2-CONSENT-001/002`, `L2-ADMIN-006`
- [ ] **H4** Media-consent enforcement on youth media `L2-CONSENT-001/004`
- [ ] **H5** Safe file/media storage (upload/sniff/limits/serve/remove) `L2-NFR-001`, `L2-CURRIC-002`, `L2-LEARN-006`, `L2-PLATFORM-002/005`
- [ ] **H6** Curriculum mutation lifecycle completion `L2-CURRIC-002..006`
- [ ] **H7** Shared progress model/views `L2-ADMIN-004`, `L2-LEARN-009/010`, `L2-GUARDIAN-001`, `L2-REPORT-001`
- [ ] **H8** Grading outcomes (rubric validation, badges, notifications, live) `L2-MENTOR-003`, `L2-LEARN-007`, `L2-ENGAGE-008`
- [ ] **H9** Automated at-risk evaluation `L2-MENTOR-007`
- [ ] **H10** Assignment side effects (journey, message, reminders) `L2-MENTOR-008`, `L2-ENGAGE-008`
- [ ] **H11** Attendance offline integrity protocol `L2-ENGAGE-003`, `L2-NFR-004/007`
- [ ] **H12** Submission concurrency (version tokens) `L2-NFR-006`
- [ ] **H13** Event/reminder generation + preference keys `L2-ENGAGE-005/008`, `L2-PLATFORM-003`
- [ ] **H14** Frontend realtime client (`/hubs/realtime`) `L2-NFR-005`, `L2-ENGAGE-007/008`
- [ ] **H15** Security audit coverage (denied actions, tamper-evidence) `L2-AUTH-009`, `L2-CONSENT-003`
- [ ] **H16** Transport/security config (TLS, CORS per env, rate limiting) `L2-AUTH-012`, `L2-NFR-002`
- [ ] **H17** Report completeness (org/attendance, filters, PDF) `L2-REPORT-001/003`, `L2-ENGAGE-004`, `L2-ADMIN-004`
- [ ] **H18** Data-subject fulfillment + retention/anonymization `L2-NFR-003`
- [ ] **H19** Unsaved-change framework `L2-NFR-007`
- [ ] **H20** Logging/alerting (correlation ids, PII redaction) `L2-PLATFORM-008`, `L2-NFR-011`
- [ ] **H21** Operational CLI + durable DB lifecycle/migrations `L2-NFR-011`
- [ ] **H22** Performance budgets `L2-NFR-008`
- [ ] **H23** Responsive coverage (XS–XL) for every new surface `L2-NFR-009`
- [ ] **H24** Accessibility coverage (WCAG 2.1 AA) for every new surface `L2-NFR-010`
- [ ] **H25** Help center (role-tailored FAQ) `L2-PLATFORM-005`
- [ ] **H26** Privacy/data-rights request UI `L2-NFR-003`

> Note: H23/H24 (responsive + a11y) are satisfied incrementally **within** each UI slice,
> not as a separate late pass — each new surface ships XS–XL + WCAG-verified.

## Requirement conflicts (resolve to declare done)

- [ ] **X1 · Volunteer roster** — API returns names only (correct). Align `L2-AUTH-010` AC3 text
      to `L2-VOLUNTEER-001` AC2 (names only, no at-risk flag). Confirming test.
- [ ] **X2 · Curriculum-edit role** — API grants Edit curriculum to Program Administrators only
      (correct). Align `L2-CURRIC-001` AC5 text to the `L2-AUTH-009` permission matrix. Confirming test.

---

## Progress log

- 2026-07-18 — Plan created from `not-implemented.md`. Starting Phase A.
