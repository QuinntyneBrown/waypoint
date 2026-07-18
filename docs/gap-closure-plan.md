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
- [x] **A3 · Donate submission** — post pledge to `POST /api/public/donation`, record provider
      reference/result. `L2-PUB-003` AC3.
- [x] **A4 · Public content source + cohorts view** — Landing/Programs consume
      `GET /api/public/content`; About/current-cohorts view (neighbourhood, dates, lead mentor,
      status from the single authoritative record). `L2-PUB-002`.
- [x] **A5 · Apply intake completeness** — persist `whoIsFilling` and cohort/track of interest
      (may need a backend command/field change first, TDD'd server-side). `L2-INTAKE-001`.

## Phase B — Cross-app navigation & error/state wiring (small, high-leverage)

- [x] **B1 · Cross-app Sign in** — public "Sign in" opens the portal app across the deploy
      boundary; all public CTAs navigate correctly. `L2-PUB-001` AC3.
- [x] **B2 · Signed-out help links** — account-locked "Contact your mentor" and
      account-deactivated "Contact the director" target an anonymous route, not `/support`.
      `L2-AUTH-006`, `L2-PLATFORM-010`.
- [x] **B3 · Global API-error → state pages** — map API 403/404/500 to the 403/404/500 state
      pages; 500 preserves in-progress form data; 500 handler logs/alerts with correlation id.
      `L2-PLATFORM-006/007/008`, `L2-NFR-011`.

## Phase C — Non-youth portal shell + Volunteer portal

- [~] **C0 · Role portal routing** — real feature routes + shell for admin/mentor/guardian/
      volunteer (today they fall through to 404 despite `roleHome()`). *Volunteer and Guardian
      routes are live; the shell already renders every role via nav-config. Admin/Mentor routes
      still land as their portals are built.*
- [x] **C1 · Volunteer Today** — today's assigned session/logistics/count, no youth record data.
      `L2-VOLUNTEER-001`. Empty and populated states are E2E-verified.
- [x] **C2 · Volunteer schedule** — upcoming assigned sessions/status + empty state.
      `L2-VOLUNTEER-002`. Empty and populated states are E2E-verified.
- [x] **C3 · Volunteer attendance** — names-only roster, Present/Absent, all-present, save,
      offline owner-bound queue + sync, strict denial of other resources.
      `L2-VOLUNTEER-001`, `L2-NFR-004`.

> `Session.VolunteerId` assignment remains part of the Phase G calendar/session-management surface.
> A Testing-only assignment fixture now exercises the populated Volunteer paths through HTTP without
> exposing a test seam outside the Testing environment.

## Phase D — Guardian portal

- [x] **D1 · Guardian home / child progress** — read-only aggregate progress, per-module
      completion, attendance history, badges, upcoming schedule, shared check-ins. `L2-GUARDIAN-001`.
- [x] **D2 · Guardian announcements** — targeted feed, unread, mark-all-read, states. `L2-GUARDIAN-002`.
- [x] **D3 · Guardian consent/contact** — view e-sign record, edit contact/emergency, granular
      media consent, withdraw, email a record copy. `L2-GUARDIAN-003`, `L2-CONSENT-001`.
- [x] **D4 · Guardian participation-consent form + consent-request delivery** — public/tokenised
      guardian consent capture; blocked youth sends secure consent link; auto-unblock.
      `L2-CONSENT-001/002`, `L2-PLATFORM-011`.

## Phase E — Youth learning & engagement

- [x] **E1** Curriculum/track browser `L2-LEARN-001` — enrolled-only track switching,
      published module map, ordered lesson/quiz gates, prerequisite explanations, and empty state.
- [x] **E2** Module & lesson reader (+ youth lesson-content read API) `L2-LEARN-002` —
      safe scripture/teaching/objective delivery, persisted text size/bookmark/position, optional
      media retry, server-enforced gates, confirmed completion, and reflection hand-off.
- [x] **E3** Reflection editor (+ read/restore API, draft) `L2-LEARN-003`, `L2-NFR-007` —
      mood and optional responses, private-by-default sharing, assigned-mentor-only reads, explicit
      draft saves, save-before-leave guard, browser-close warning, and server draft restoration.
- [x] **E4** Quiz intro & attempt (one-question, autosave/resume) `L2-LEARN-004` —
      question/time/attempt/pass-mark/badge metadata, one-question keyboard flow, authoritative
      answer and position autosave/resume, immutable confirmation, scoring, and attempt limits.
- [x] **E5** Quiz result/review (+ review/badge result data) `L2-LEARN-005` —
      approval-withheld score, approved pass/fail, feedback-mode-aware selections/correct answers,
      once-only earned badge, best passing score, attempts remaining, and retake.
- [x] **E6** Assignment brief & submission (+ brief/read API, file upload) `L2-LEARN-006` —
      due/type/rubric brief, dynamic allowed-type and attestation validation, 10 MB JPG/PNG
      MIME/extension/magic-byte validation, private authenticated storage, confirmation, and copy lock.
- [x] **E7** Submission status/resubmission (+ enriched status data) `L2-LEARN-007` —
      submitted copy, durable lifecycle timeline, mentor feedback, rubric points, assignment badge,
      status notification/realtime publish, revision editor, and return to review on resubmission.
- [x] **E8** Coding lesson/project `L2-LEARN-008` — authored ordered steps and code samples,
      resumable editor with sandboxed live preview, server-validated self-check progress, once-only
      completion badge, responsive keyboard journey, and project submission with an optional mentor
      note through the assignment review lifecycle.
- [x] **E9** Journey & badges (counted criteria, milestones) `L2-LEARN-009` — shared
      curriculum snapshot with authoritative completion counts/current module, track-scoped earned and
      locked badges, rule-aware counted progress, earn dates, joined/earned/next milestone timeline,
      and responsive journey and collection routes.
- [x] **E10** Personal progress (quiz history, attendance tabs, competencies) `L2-LEARN-010` —
      authoritative lesson totals, approved-score average/history and pass state, per-session attendance
      with excused-aware rate, latest named mentor competency level/percent, and responsive semantic tabs.
- [x] **E11** Youth schedule & RSVP (youth-visible calendar, reminders) `L2-ENGAGE-005/008` —
      future same-cohort sessions with logistics/current response, one-record RSVP updates, optional
      mentor note, durable preference-aware reminders, and responsive keyboard/touch acceptance.
- [x] **E12** Youth messaging (mentor thread + realtime) `L2-ENGAGE-007`, `L2-NFR-005` —
      assigned-mentor identity and one private thread, unread clearing, safe text rendering, send/receive,
      recipient-scoped live delivery, reconnect reconciliation, and responsive keyboard composer.
- [x] **E13** Youth home placed-state content + `/youth/messages` link fix `L2-LEARN-*`, `L2-PLATFORM-011` —
      authoritative current-learning action, shared curriculum progress, next cohort session/RSVP,
      mentor-assigned work with due/status links, mentor support, and preserved no-cohort guidance.
- [x] **E14** Youth notifications deep links + live events `L2-ENGAGE-008`, `L2-NFR-005` —
      actionable unread cards route to their target screens, recipient-scoped events appear live,
      feed/reconnect reconciliation preserves authoritative read state without duplicates, and the
      responsive notification target is keyboard operable.

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

- [~] **G1** Application review `L2-INTAKE-002` — server list query (`GET /api/applications`,
      admin-only) + admin review-queue UI (`/admin/applications`), E2E-verified end to end
      (anonymous submit → queue). Detail view + approve/decline UI + "Needs consent" gating still
      to add (approve/decline commands already exist server-side).
- [x] **G2** People directory `L2-ADMIN-001` — list + server-side search + empty-state/invite CTA,
      E2E-verified (admin routing + `/admin/people`). Role/cohort filter chips still to add.
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
- [~] **H5** Safe file/media storage (upload/sniff/limits/serve/remove) `L2-NFR-001`, `L2-CURRIC-002`, `L2-LEARN-006`, `L2-PLATFORM-002/005` — assignment evidence now uses private DB-backed JPG/PNG storage with MIME/extension/signature/size checks and authenticated no-sniff serving; curriculum media lifecycle/removal remains.
- [ ] **H6** Curriculum mutation lifecycle completion `L2-CURRIC-002..006`
- [~] **H7** Shared progress model/views `L2-ADMIN-004`, `L2-LEARN-009/010`, `L2-GUARDIAN-001`, `L2-REPORT-001` — curriculum and journey share one module completion/current-state snapshot, badge counts derive from authoritative progress records, and self-analytics now includes approved quiz, attendance, and latest competency histories; cross-role histories and report unification remain.
- [~] **H8** Grading outcomes (rubric validation, badges, notifications, live) `L2-MENTOR-003`, `L2-LEARN-007`, `L2-ENGAGE-008` — youth-visible rubric/badge results plus notification and user-scoped realtime publishing are in place; mentor review UI and stricter rubric-total reconciliation remain.
- [ ] **H9** Automated at-risk evaluation `L2-MENTOR-007`
- [ ] **H10** Assignment side effects (journey, message, reminders) `L2-MENTOR-008`, `L2-ENGAGE-008`
- [x] **H11** Attendance offline integrity protocol `L2-ENGAGE-003`, `L2-NFR-004/007` —
      owner-bound private queue, explicit durable idempotency receipts, automatic ordered replay,
      pending count, optimistic conflict choice, and route/browser unsaved-change warning.
- [ ] **H12** Submission concurrency (version tokens) `L2-NFR-006`
- [~] **H13** Event/reminder generation + preference keys `L2-ENGAGE-005/008`, `L2-PLATFORM-003` —
      session RSVP reminders now schedule, cancel, dispatch once, deep-link to the schedule, and obey
      the canonical `session-reminders` in-app preference; direct-message notifications now use the
      matching `mentor-messages` key. Other required event generators and email delivery remain.
- [x] **H14** Frontend realtime client (`/hubs/realtime`) `L2-NFR-005`, `L2-ENGAGE-007/008` —
      one token-authenticated app-level connection starts after sign-in, reconnects with the current
      token, reconciles threads without duplicates, updates youth messages, and tears down with the
      session. Recipient-scoped notifications merge into the live feed, reconcile from the server on
      reconnect without duplicates, and preserve server-authoritative read state.
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

- [x] **X1 · Volunteer roster** — aligned `L2-AUTH-010` AC3 to `L2-VOLUNTEER-001`
      AC2 (names only, no at-risk flag); confirming test passes.
- [x] **X2 · Curriculum-edit role** — aligned `L2-CURRIC-001` AC5 to the `L2-AUTH-009`
      permission matrix (Program Administrators only); confirming authorization tests pass.

---

## Foundations (test infrastructure, not an L2 gap)

- [x] **E2E seeding** — `POST /api/_test/users` (Testing environment only) provisions active
      non-Youth identities so the HTTP-only Playwright harness can reach the Admin/Mentor/
      Guardian/Volunteer portals. E2E helpers: `registerUser(role)`, `signInApi(creds)`.
- [x] **Volunteer assignment fixture** — `POST /api/_test/volunteer-sessions` creates a populated,
      assigned session in Testing only so Today, schedule, attendance, offline, and conflict paths
      execute through real HTTP and product APIs.
- [x] **Guardian family + email fixtures** — Testing-only family seeding covers aggregate, empty,
      consented, and consent-required states; the Testing-only email sink lets Playwright follow the
      delivered one-time link without exposing capability tokens in product responses.
- [x] **Youth curriculum fixture** — Testing-only multi-track enrollment data covers published/draft
      filtering, ordered lesson/quiz gates, prerequisite locking, progress, safe lesson content,
      failed optional media, reflection hand-off, upcoming cohort logistics, an assigned-mentor thread,
      mentor-assigned work, the placed dashboard, and an empty enrolled track.

## Progress log

- 2026-07-18 — Plan created from `not-implemented.md`. Starting Phase A.
- 2026-07-18 — A1/A2/A3 done: L2-PUB-003 wired end to end (contact, volunteer, donation).
- 2026-07-18 — Seeding foundation landed (Testing-only user seed endpoint) — unblocks the
  authenticated non-Youth portals.
- 2026-07-18 — A4/A5 done: public marketing pages now render the authoritative content payload,
  `/about` lists privacy-safe current cohorts, and intake persists who submitted it plus cohort
  interest with end-to-end validation.
- 2026-07-18 — B1/B2/B3 done: runtime-configured links cross the public/portal boundary, signed-out
  recovery links reach anonymous contact, API 403/404/500 responses map to shared states, transient
  form drafts recover after a 500, and error responses/log alerts carry correlation IDs.
- 2026-07-18 — Removed the local announcement-suite exception: role-target filtering now uses an
  EF-translatable list on the preview runtime. All 141 backend integration tests pass with the
  environment's InMemory fallback.
- 2026-07-18 — C1/C2/C3 and H11 done: populated Volunteer Today/schedule paths are verified;
  attendance is names-only with Present/Absent bulk capture, scoped saves, durable idempotency,
  owner-bound offline replay, visible pending state, deterministic conflict choice, and unsaved
  navigation protection. The two governing requirement conflicts (X1/X2) are also reconciled.
- 2026-07-18 — D1/D2/D3/D4 done: the Guardian portal now provides scoped aggregate progress,
  targeted news, contact/media management, consent receipts/copies/withdrawal, and responsive mobile
  paths. Blocked youth can deliver a hashed, expiring, one-time consent link; public typed e-signature
  capture emails a receipt and unblocks participation on the next server-authoritative access check.
- 2026-07-18 — E1 done: youth curriculum now discovers only enrolled published tracks, switches
  tracks, shows ordered per-item and per-module progress/gates with explanations, denies cross-track
  and locked direct requests server-side, and is verified at desktop and mobile keyboard seams.
- 2026-07-18 — E2/E3 done: the youth lesson reader now delivers safe published content with
  persisted reading controls and resilient optional media, confirms completion, and opens a real
  reflection editor. Reflections validate text/mood on submission, retain optional responses, remain
  private by default, restore server drafts, save before guarded navigation, and expose submitted
  shared entries only to the youth's assigned mentor. Both surfaces pass desktop and mobile keyboard
  acceptance paths.
- 2026-07-18 — E4/E5 done: youth quizzes now provide complete intro rules and rewards, resume the
  same one-question position with serialized server autosaves, confirm immutable submission, enforce
  configured limits, and remain score-hidden until mentor approval. Approved results apply feedback
  mode, identify selected/correct answers, display the once-only badge and best passing score, and
  start a permitted retake. Desktop and mobile keyboard journeys exercise the real approval path.
- 2026-07-18 — E6/E7 done: assignments now expose task/due/type/rubric briefs, validate attested
  allowed content, store signature-checked JPG/PNG evidence privately, and lock the submitted copy.
  Youth status shows the durable review/revision timeline, submitted content, mentor comment, rubric
  points, and earned badge; grading produces a notification and scoped realtime event, and the
  revision editor returns resubmitted work to review. Desktop and mobile acceptance paths pass.
- 2026-07-18 — E11 done: youth can see only upcoming sessions in their own cohort, submit and revise
  one RSVP with an optional mentor-visible note, and schedule or cancel a 24-hour reminder. Due
  reminders are consumed once into the preference-aware notification feed, and desktop/mobile
  acceptance verifies persistence, touch containment, and keyboard operation.
- 2026-07-18 — E12 done: youth messaging resolves only the assigned mentor, marks inbound messages
  read when the thread opens, safely renders and sends text, and receives recipient-scoped SignalR
  events through an app-level token-aware reconnecting client. Reconnects reconcile from the server
  without duplicates; desktop live-reply and mobile keyboard paths pass.
- 2026-07-18 — E13 done: the placed youth home now combines the next accessible curriculum item,
  calculator-backed progress, same-cohort next session and RSVP, and youth-scoped assigned work with
  resolved due/status destinations. The no-cohort mentor path now reaches the delivered thread;
  desktop, mobile reflow/keyboard, and no-cohort regression paths pass.
- 2026-07-18 — E14 and H14 done: the youth notification feed now consumes recipient-scoped live
  events, merges in-flight feed and reconnect responses by durable ID, retains authoritative unread
  state, and renders actionable cards as accessible deep links. Desktop acceptance proves live
  arrival, reload deduplication, and target-thread routing; mobile acceptance proves containment,
  touch size, and keyboard activation.
