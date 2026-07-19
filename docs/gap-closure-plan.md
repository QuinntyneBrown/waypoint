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
      routes are live; Mentor dashboard/roster and the first Admin routes now land, and the shell
      already renders every role via nav-config. Remaining Admin/Mentor destinations land as their
      portal slices are built.*
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

- [x] **F1** Dashboard/roster/reviews preview `L2-MENTOR-001` — assigned-youth-only summary
      counts, overdue review totals, progress and at-risk status, server-side roster search/status
      filters, labelled populated/empty states, and assignment/quiz review previews with grading links.
- [x] **F2** Youth detail (scoped tabs) `L2-MENTOR-002` — assigned-youth-only profile and
      guardian contact, authoritative progress, attendance, privacy-trimmed check-in history,
      submission and competency tabs, plus named/timestamped at-risk attribution.
- [x] **F3** Submission review/grading (+ queue/detail API, outcomes) `L2-MENTOR-003`, `L2-LEARN-007` —
      scoped pending queue and submitted-copy detail, persisted criterion drafts, server-validated
      rubric bounds/computed totals, confirmed approval or mandatory-feedback revision, default-on
      guardian choice, badges, audit, and recipient-scoped youth/guardian notifications.
- [x] **F4** Quiz assessment approvals (+ pending/review queries) `L2-MENTOR-004` —
      scoped pending queue with authored quiz titles, answer/correctness detail, auto-score approval,
      validated override, immutable auto/approved audit record, and confirmed scoped approve-all with
      one durable audit entry per assessment.
- [x] **F5** Competency tracker `L2-MENTOR-005` — complete ordered staged framework including
      unassessed rows, four-level assessment and notes, append-only history with latest assessor/time,
      live proficient/mastered summary, assigned-youth scoping, and shared latest youth-progress values.
- [x] **F6** Check-ins & history (+ shared read surfaces) `L2-MENTOR-006` — assigned-youth
      journal with required date/type, full conversation fields, follow-up and guardian-informed state,
      private-by-default explicit sharing (including a prayer disclosure warning), scoped youth/type
      filters and full mentor history, plus shared-only youth and guardian projections.
- [x] **F7** At-risk flags + automated evaluation `L2-MENTOR-007` — scoped active list with
      specific combined signal reasons, manual raise, youth/detail/check-in/quick-message actions,
      confirmed four-choice resolution, director visibility, and private-detail audit events.
- [x] **F8** Assign work (+ journey placement, message, reminders) `L2-MENTOR-008`, `L2-ENGAGE-008` —
      scoped published module/lesson/project picker, individual or whole-cohort recipients, due date
      and reminder cadence, optional personal message, responsive confirmation count, and durable
      youth journey/message/notification placement.
- [x] **F9** Mentor calendar & attendance `L2-ENGAGE-001..004` — scoped named month/week/agenda
      views, cohort filtering, mentor-owned weekly recurrence through term end, attendance links,
      four-state roster capture with notes/bulk tallies, shared offline/concurrency recovery, filtered
      session/youth rates and trends, and filter-honouring CSV/PDF exports.
- [x] **F10** Mentor messaging/notifications `L2-ENGAGE-007/008`, `L2-NFR-005` — active
      assigned-youth inbox with cohort names, previews and unread counts, participant-scoped private
      threads, per-cohort personal-message broadcast with confirmation count, preference-aware live
      delivery, reconnect reconciliation, and a role-correct mentor notification feed/read state.

## Phase G — Program Administrator portal

- [x] **G1** Application review `L2-INTAKE-002` — admin-only effective-status filtering and full
      applicant/guardian detail, track/cohort/eligible-mentor placement, server-enforced guardian
      consent gate, confirmed approve/decline outcomes, cohort-track enrollment, contact email, and
      content-free decision audits, with desktop/mobile review acceptance.
- [x] **G2** People directory `L2-ADMIN-001` — debounced server search, composable role chips and
      named cohort filter, reset-safe ten-row paging, total/page state, responsive table-to-cards,
      labelled empty/error recovery, and invite CTA.
- [x] **G3** Person detail / account lifecycle `L2-ADMIN-002` — linked responsive person records
      provide profile, guardian/consent, authoritative curriculum progress, attendance, privacy-safe
      operational notes, and account tabs; audited reset/role/lifecycle actions use explicit warnings
      and confirmations, revoke current sessions immediately, preserve reporting records, and enforce
      irreversible removal as a distinct historical state excluded from active directory use.
- [x] **G4** Cohorts & cohort detail `L2-ADMIN-003` — responsive populated/empty/retry list,
      authoritative enabled-track/active-mentor creation options, dates/location capture, named
      detail, audited lead/name/location edits, and integrity-checked existing-youth membership with
      track enrollment, live counts, person-record links, and an invitation hand-off.
- [x] **G5** Progress board (+ live, export) `L2-ADMIN-004`, `L2-REPORT-003` — selected
      cohort/track matrices derive authoritative assignment, lesson completion, quiz and aggregate
      states; role-scoped progress events refresh the current board live, while filtered CSV/PDF
      exports and contained sticky-column mobile scrolling cover reporting and narrow screens.
- [x] **G6** Mentor assignment / caseload `L2-ADMIN-005` — a responsive admin workspace pairs
      active youth with active mentors, replaces audited cohort lead/supporting teams independently,
      and shows named caseload/check-in/review metrics against a shared 4–8 target band with explicit
      under-target, balanced, and overloaded states.
- [x] **G7** Organization settings + term lifecycle `L2-ADMIN-006` — Program Administrators can
      maintain audited organization identity/contact and consent policy, create and advance uniquely
      named dated terms, and pause or resume published tracks without losing progress. The configured
      identity, contact, statement, re-consent rule, and media policy drive the guardian-consent flow;
      disabled tracks reject new enrollment and work assignment while existing records remain intact.
- [x] **G8** Curriculum builder `L2-CURRIC-001..006` — Program Administrators now have
      responsive track, module, lesson/media, quiz/question, assignment/rubric, badge, and
      progression workspaces. Exact-set ordering preserves prerequisites, publish actions expose
      authored work to the intended youth exactly once, badge rules support scoped automatic and
      manual awards, and the staged competency framework drives the mentor's current-stage view.
- [x] **G9** Calendar/session management `L2-ENGAGE-001` — the shared staff calendar now gives
      Program Administrators organization-wide month, week, and agenda views with cohort filters,
      named time/location/mentor/cohort detail, role-correct attendance links, and a validated
      recurring-session form that materializes weekly rows only through the selected cohort term.
- [x] **G10** Attendance administration/history `L2-ENGAGE-002..004` — Program Administrators
      now use the shared staff capture and history workspaces across the organization, with four-state
      marks, staff notes, bulk actions, live tallies, optimistic versions, private offline replay,
      cohort/date filters, session and youth rates/trends/flags, and exact-filter CSV/PDF exports.
- [x] **G11** Announcement administration `L2-ENGAGE-006` — Program Administrators can compose
      validated organization or cohort announcements for Youth, Youth + Guardians, Guardians,
      Mentors, or Everyone; send immediately or schedule future delivery; and review sent/scheduled
      history with authoritative recipient, read, and read-rate statistics across responsive layouts.
- [x] **G12** Reports hub + standard reports `L2-REPORT-001` — the responsive reports hub now
      applies one date/cohort/track/mentor scope to organization KPIs, attendance, cohort, and staff
      charts with authoritative timestamps and record counts. Detailed youth and cohort reports add
      curriculum, quiz, competency, attendance, badge, mentor-summary, status, retention, and
      comparison views; empty data, global error recovery, and mentor ownership boundaries are tested.
- [x] **G13** Data export (PDF + targets) `L2-REPORT-003` — overview, youth, cohort, staff,
      attendance, progress-board, and audit-log exports now provide real CSV/PDF files using the
      active filters and authoritative role/resource scope. Every export is audited, PDF output is
      valid, and formula-like CSV values are neutralized before spreadsheet delivery.
- [x] **G14** Roster import UI `L2-REPORT-004` — admins now upload locally validated CSV files up
      to 2 MB, see exact row/column counts, map or skip unique columns, and review authoritative
      Ready/Duplicate/Missing consent/Invalid classifications before an explicit ready-only commit.
      Imports resolve named cohort/mentor/track relationships, remain idempotent by email, report all
      outcomes, and write a count-bearing audit record across responsive layouts.
- [x] **G15** Audit log UI `L2-CONSENT-003` — Program Administrators can inspect a newest-first,
      paged audit trail with stable actor/action options and inclusive date filters, named actors,
      source and target context, explicit private markers, and redacted safeguarding details. Empty,
      loading, error/retry, responsive card-table, pagination, and exact-filter audited CSV export
      states are covered; other roles remain denied by the authoritative audit policy.

## Phase H — Cross-cutting backend & platform

- [x] **H1** Idle expiry + draft recovery framework `L2-AUTH-007`, `L2-PLATFORM-010`, `L2-NFR-007` —
      one app-wide 30-minute deliberate-activity timer now revokes the server session, falls back to
      local teardown when disconnected, preserves owner-bound safe form fields, bypasses dirty-route
      guards for the authoritative expiry state, and carries the recovery route through sign-in.
      Reflection recovery restores text, mood, optional response, and sharing choice after its async
      editor loads; explicit sign-out still purges all Word Up session and draft storage.
- [x] **H2** Per-action consent enforcement in protected youth handlers `L2-CONSENT-002/004` —
      every lesson, quiz, assignment, coding, reflection, journey, and progress request now applies
      the shared participation policy before its handler runs. Linked minors without current consent
      receive an authoritative consent-required state, withdrawal blocks the next action, and fresh
      consent unblocks the same session; guardianless adult Youth remain outside the minor gate.
- [x] **H3** Correct consent validity (flag/withdrawal/window/term/re-consent) `L2-CONSENT-001/002`, `L2-ADMIN-006` —
      one authoritative policy evaluator now applies participation choice, withdrawal, validity window,
      active-term identity, and the organization re-consent setting consistently to access, consent
      status, application decisions/listing, and administrative person records; new records capture
      the current term and policy-derived validity window.
- [x] **H4** Media-consent enforcement on youth media `L2-CONSENT-001/004` — the shared
      term-aware evaluator now distinguishes individual photo/video, group media, and first-name
      caption grants. Individual assignment evidence is checked before storage and submission;
      group-only consent is insufficient, media-only withdrawal immediately blocks the private file
      and removes its URL from Youth and staff projections, and guardianless adult Youth remain open.
- [x] **H5** Safe file/media storage (upload/sniff/limits/serve/remove) `L2-NFR-001`,
      `L2-CURRIC-002`, `L2-LEARN-006`, `L2-PLATFORM-002/005` — assignment evidence uses purpose-aware
      private DB-backed JPG/PNG/PDF storage with MIME/extension/signature/10 MB checks, owner-bound
      submission linkage, unused-upload cleanup, and authenticated no-sniff serving to the Youth,
      assigned mentor, or administrator. Lesson media uses private validated PDF/MP4 storage, and
      profile photos use owned, replaceable 5 MB JPG/PNG storage across onboarding and settings with
      self/guardian/assigned-mentor/admin read scope and live minor media-consent enforcement. Support
      requests now link optional private 5 MB JPG/PNG screenshots with preview/removal, abandoned-draft
      cleanup, owner access, and administrator access only after submission; external media URLs are
      no longer accepted by any of these flows.
- [x] **H6** Curriculum mutation lifecycle completion `L2-CURRIC-002..006` — lesson content
      and media update/removal, exact lesson/question ordering, typed quiz validation and short-answer
      scoring, assignment update/publish/fan-out, competency soft removal, and module/attendance/
      assignment automatic badge rules now have validated, audited, exact-once server lifecycles.
- [x] **H7** Shared progress model/views `L2-ADMIN-004`, `L2-LEARN-009/010`, `L2-GUARDIAN-001`, `L2-REPORT-001` —
      curriculum and journey share one module completion/current-state snapshot, badge counts derive
      from authoritative progress records, and self-analytics includes approved quiz, attendance, and
      latest competency histories. One role-neutral outcome calculator projects approved quiz history
      and latest competency levels to Youth and Guardian views, while pending attempts, quiz answers,
      and private assessment notes remain excluded. Guardian attendance and shared check-ins plus Admin
      and mentor youth/cohort/organization reports complete the cross-role histories.
- [x] **H8** Grading outcomes (rubric validation, badges, notifications, live) `L2-MENTOR-003`, `L2-LEARN-007`, `L2-ENGAGE-008` — rubric IDs/ranges and complete final scores are server-validated,
      totals are server-computed, configured badges and youth-visible results are applied, and final
      outcomes publish durable user-scoped youth plus opt-in guardian notifications in real time.
- [x] **H9** Automated at-risk evaluation `L2-MENTOR-007` — shared dashboard/roster/detail/list
      evaluation for configurable inactivity, missed-session, and cohort-progress-gap thresholds;
      one active flag per youth, unchanged-signal suppression after resolution, and re-arming after
      signals clear or change.
- [x] **H10** Assignment side effects (journey, message, reminders) `L2-MENTOR-008`, `L2-ENGAGE-008` —
      each recipient receives independently scoped journey work, an optional real mentor-thread
      message, and a once-consumed due reminder with actionable deep links and preference enforcement.
- [x] **H11** Attendance offline integrity protocol `L2-ENGAGE-003`, `L2-NFR-004/007` —
      owner-bound private queue, explicit durable idempotency receipts, automatic ordered replay,
      pending count, optimistic conflict choice, and route/browser unsaved-change warning.
- [x] **H12** Submission concurrency (version tokens) `L2-NFR-006` — the shared Submission
      aggregate exposes one provider-independent version to mentor review and Youth status clients;
      every grading and resubmission update requires that version, advances it on success, and rejects
      stale or database-raced writes with 409. Both editors retain their local draft and explicitly
      load the latest review/feedback before reconciling and retrying.
- [x] **H13** Event/reminder generation + preference keys `L2-ENGAGE-005/008`, `L2-PLATFORM-003` —
      graded work, earned badges, new messages, due-soon work, session reminders, automatic at-risk
      flags, and review-ready assignment/quiz submissions now target the correct user with actionable
      deep links and canonical event keys. One delivery boundary applies independent in-app and email
      preferences, publishes enabled in-app events live, suppresses disabled channels, and consumes
      scheduled reminders once; onboarding/settings expose every implemented event category.
- [x] **H14** Frontend realtime client (`/hubs/realtime`) `L2-NFR-005`, `L2-ENGAGE-007/008` —
      one token-authenticated app-level connection starts after sign-in, reconnects with the current
      token, reconciles youth and mentor threads/inboxes without duplicates, and tears down with the
      session. Recipient-scoped notifications merge into each role's live feed, reconcile from the
      server on reconnect without duplicates, and preserve server-authoritative read state.
- [x] **H15** Security audit coverage (denied actions, tamper-evidence) `L2-AUTH-009`, `L2-CONSENT-003` —
      authenticated endpoint-policy and application resource denials write actor/source-scoped events
      without request bodies; every named mutation uses the same writer. Audit rows form a signed,
      sequenced HMAC chain, EF rejects update/delete attempts, and the admin view verifies the entire
      chain and raises an incident warning when an unsigned, changed, removed, or forged row is found.
- [x] **H16** Transport/security config (TLS, CORS per env, rate limiting) `L2-AUTH-012`, `L2-NFR-002` —
      production rejects plain HTTP before credentials/tokens reach handlers and accepts only configured
      HTTPS CORS origins; Development has its own localhost policy. One configurable IP window covers
      sign-in/auth, password-reset, invitation, and staff-triggered reset routes with 429 + Retry-After.
- [x] **H17** Report completeness (org/attendance, filters, PDF) `L2-REPORT-001/003`, `L2-ENGAGE-004`, `L2-ADMIN-004` —
      the standard organization, youth, cohort, attendance, and staff views now share date/cohort/
      track/mentor filters, KPIs, named activity details, counts/timestamps, status and retention,
      program/selected/previous comparisons, and mentor scope. All standard reports plus attendance,
      progress-board, and audit-log data export through filtered, authorized, audited CSV/PDF files.
- [x] **H18** Data-subject fulfillment + retention/anonymization `L2-NFR-003` —
      authenticated access, correction, removal, and consent-withdrawal requests now create encrypted,
      trackable workflow records that administrators can discover and fulfil once with content-free,
      tamper-evident audit evidence. Correction updates requested profile fields; withdrawal closes active
      subject consent; removal revokes sessions and erases private content/files while retaining
      pseudonymous attendance, progress, score, and status rows. A configurable retention run applies the
      same anonymizer only to expired deactivated/removed accounts, preserving de-identified reports;
      AES-GCM converters protect privacy requests, messages, check-ins, reflections, submissions,
      attendance notes, support content, guardian contacts, and signatures at rest with tamper detection.
- [x] **H19** Unsaved-change framework `L2-NFR-007` — one reusable route/browser-close adapter now
      protects reflection, mentor/volunteer attendance, grading, badge, progression, and multi-form
      curriculum editors. Navigation offers save-draft or stay; synchronous owner-bound local drafts
      and grading/reflection server drafts restore on return without exposing another user's work.
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
      mentor-assigned work, overdue rubric and pending answer-backed quiz reviews, assessed/unassessed
      competencies, an active flag, guardian contact, a private check-in and explicit-sharing journey,
      the placed dashboard, and an empty enrolled track.

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
- 2026-07-18 — F1 done: mentor sign-in now lands on a scoped production dashboard with caseload,
  pending/overdue review, average-progress, and at-risk metrics. The dashboard previews assigned youth
  and assignment/quiz reviews; My Youth provides debounced server search and all/on-track/at-risk/new
  filters with labelled empty states. Integration acceptance excludes another mentor's youth, flags,
  and overdue work; desktop and mobile keyboard/reflow paths pass.
- 2026-07-18 — F2 done: an assigned youth's mentor-only record now provides semantic tabs for
  profile/guardian contact, curriculum-aware lesson progress, attendance, privacy-trimmed check-ins,
  submissions, and latest named competencies. Active risk banners identify the reason, flagger, and
  date; HTTP tests preserve the cross-mentor 403 and prove private prayer text is excluded, while
  desktop/mobile acceptance covers every populated tab and keyboard reflow.
- 2026-07-18 — F3 and H8 done: mentors now work a scoped review queue, inspect the immutable submitted
  copy, score the authored rubric, retain drafts without changing youth status, and confirm approval or
  feedback-required revision. The API rejects unknown, out-of-range, or incomplete final scores and
  computes totals itself; badge, audit, youth result/revision, durable youth/optional-guardian
  notification, and user-scoped live outcomes are exercised by HTTP plus desktop/mobile acceptance.
- 2026-07-18 — F4 done: pending quiz assessments now carry their authored titles into the queue and
  open a scoped answer-by-answer review with explicit selected/correct states. Mentors can approve the
  auto-score, enter a server-validated override, inspect the immutable auto/approved/actor/time record,
  or confirm approve-all; youth results expose only the approved score, while per-attempt and bulk
  audit records preserve every action. Desktop/mobile acceptance and all mentoring HTTP tests pass.
- 2026-07-18 — F5 done: each assigned youth now has an ordered staged competency tracker covering
  Not started through Mastered, including previously unassessed framework rows, notes, latest assessor
  and timestamp, and a live proficient/mastered summary. Saves append history while mentor and youth
  views resolve the latest assessment; HTTP scoping/history/downstream tests and desktop/mobile
  keyboard acceptance pass.
- 2026-07-18 — F6 done: mentors now have an assigned-youth journal for dated 1:1, call, and session
  notes, goals, prayer requests, actions, follow-up, and guardian-informed state, with youth/type
  filters over the full scoped history. Entries remain private by default; the explicit sharing control
  warns that every entered field, including prayer, reaches the youth and linked guardian. HTTP tests
  prove cross-mentor denial and private/shared projection boundaries, while desktop cross-role and
  mobile keyboard/reflow acceptance pass.
- 2026-07-18 — F7 and H9 done: dashboard, roster, youth detail, and the at-risk workspace now share
  configurable evaluation of seven-day inactivity, one missed session, and progress more than 15%
  below the cohort average. Combined reasons attach to one active flag; resolution suppresses unchanged
  signals until they clear or change, allowing genuine recurrence without duplicate flags. Mentors can
  raise concerns, message, log a check-in, or confirm one of four resolution outcomes; directors see
  all flags and private-detail audit events remain redacted. HTTP lifecycle/configuration tests and
  desktop/mobile keyboard acceptance pass.
- 2026-07-18 — F8 and H10 done: mentors can select published work from the tracks available to their
  scoped caseload, target individual youth or an entirely owned cohort, set a due date and one/two-day
  reminder, add a personal message, and confirm the exact recipient count. The command rejects partial,
  cross-mentor, unpublished, and out-of-journey targets before writing; accepted work reaches each
  youth journey, real mentor thread, and preference-aware immediate/due notification feed. HTTP tests,
  portal unit/build checks, and desktop/mobile keyboard acceptance pass.
- 2026-07-18 — F9 done and H17 advanced: mentors now have scoped month/week/agenda calendars with
  cohort filters, named logistics, attendance links, and a required cohort/mentor session form that
  materializes weekly recurrence only through term end. Session rosters support Present/Late/Excused/
  Absent, staff notes, bulk actions, live tallies, optimistic versions, unsaved warnings, and the same
  private owner-bound offline queue used by volunteers. Filtered session and youth history exposes
  saved/review/low-turnout states, rates, trends, and below-threshold flags; CSV and valid PDF exports
  honour the active filters. Cross-mentor denials, desktop/mobile keyboard flows, offline replay, shared
  volunteer regression, 30 engagement integrations, portal units, and the production build pass.
- 2026-07-18 — F10 done: mentors now have a named active-roster inbox with previews and unread
  counts, scoped personal threads, and cohort broadcasts that confirm the exact recipient count and
  fan out into private messages. Direct and broadcast messages safely persist, respect in-app
  preferences, publish recipient-only live message/notification events, and use role-correct deep
  links; broadcasts also leave a content-free audit record. The mentor bell opens the shared live feed
  with authoritative mark-all-read state. Two HTTP acceptances, all 22 engagement integrations, six
  portal units, the production build, a two-session desktop realtime journey, and mobile keyboard/
  touch/reflow acceptance pass.
- 2026-07-18 — G1 done: the application queue now filters on effective New/In review/Needs consent/
  Approved/Declined state and opens complete applicant, motivation, acknowledgement, guardian, and
  placement detail. Minor applications without a current participation-consent record are visibly
  gated and cannot provision an active account. Eligible approval requires a real cohort and one of
  its active mentors, creates or links the youth, enrolls the cohort track, stores the decision, sends
  an outcome email, and audits the action; confirmed decline keeps its optional note, emails the
  contact, and creates no account. Nine intake integrations plus desktop and mobile keyboard/touch/
  reflow journeys pass.
- 2026-07-18 — G2 done: the people directory now composes debounced name/email search with accessible
  role chips and named cohort selection, resets safely to page one when filters change, and exposes
  deterministic ten-row previous/next navigation with totals. Results include cohort names and reflow
  from a table into labelled cards; empty/error states retain filter recovery and the invite action.
  HTTP role/cohort coverage, populated desktop paging/filter acceptance, mobile touch/reflow, portal
  units, and the production build pass.
- 2026-07-18 — G3 done: every directory row now opens a responsive record with profile, named
  guardian/contact and current consent, shared curriculum progress, attendance rate/history,
  privacy-safe operational notes, and account state. Password reset and role changes are audited;
  staff-role warnings and deactivate/reactivate/remove confirmations state their consequences, while
  a session-version claim makes deactivation, password/session revocation, and role changes effective
  on the next protected request. Permanent removal is a distinct, non-reactivatable historical state
  excluded from the active directory while retaining progress for reports. All 177 backend
  integrations, four backend units, six portal units, the production build, and desktop/mobile
  acceptance pass.
- 2026-07-18 — G4 done: admins can create cohorts only from enabled published tracks and active
  mentors, capture term dates/location, browse named cohort cards, and recover list loads or start
  from a labelled empty state. Detail pages show the track, lead, term, location, member count, and
  status-aware roster; audited edits keep the cohort lead association synchronized, while adding an
  unassigned active youth rejects wrong roles and cross-cohort moves, enrolls the cohort track, and
  updates membership immediately. Six cohort integrations, the production build, and desktop/mobile
  creation, detail, edit, membership, empty, retry, keyboard, touch, and reflow acceptance pass.
- 2026-07-18 — G5 done: admins can select a cohort and any enabled published track to see
  authoritative done, in-progress, not-started, and not-assigned states with per-youth, per-module,
  quiz, and cohort aggregates. Lesson and coding mutations publish role-scoped progress events that
  refresh the selected matrix without navigation; audited CSV and PDF downloads preserve the active
  filters, and the sticky matrix scrolls within its container on narrow screens. Seven cohort and ten
  learning integrations, six portal units, the production build, and three live/export/mobile browser
  scenarios pass. G13 is partial because other export targets remain.
- 2026-07-18 — G6 done: admins can pair active youth with validated active mentors and see the youth
  on the new mentor's roster immediately. Each cohort now has an integrity-checked, audited team with
  one lead and independent supporting mentors, including safe promotion from supporting to lead and
  different roles for the same mentor across cohorts. The workspace shows active-youth caseload,
  check-ins, reviews, the shared 4–8 target band, and overload warnings with empty/error recovery and
  mobile reflow. Twenty-four adjacent backend integrations, six portal units, the production build,
  and three pairing/cohort-role/mobile browser scenarios pass.
- 2026-07-18 — G7 and H3 done: audited Program Administrator settings now cover organization name,
  display name, logo, city, contact, guardian statement, per-term re-consent, and media policy, with
  the configured identity and contact carried into public consent forms, receipts, and emails. Admins
  can create/edit validated Upcoming, Active, and Completed terms and pause or resume published tracks;
  pausing retains progress while server-side enrollment, application placement, and mentor work
  assignment reject new use. Consent capture records the authoritative active term and validity window,
  and one shared evaluator applies participation, withdrawal, dates, term, and policy across all status
  and administrative projections. Thirty-six adjacent backend integrations, six portal units, the
  production build, and three consent/term/track/mobile browser scenarios pass.
- 2026-07-18 — G8 and H6 done; H5 advanced: admins can author complete ordered tracks with modules,
  lessons, safe private PDF/MP4 media, typed quizzes including short answers, rubric assignments,
  automatic/manual badge definitions, and ordered progression stages/competencies from three
  responsive workspaces. Publishing fans assignments to enrolled youth exactly once; lesson,
  module, quiz, assignment, and attendance events award scoped badges with durable and live
  notifications exactly once, while archived competencies disappear from the active framework and
  mentor current-stage calculations. Youth text answers autosave/resume and score case-insensitively.
  All 199 backend integrations, four backend units, six portal units, the production build, and seven
  combined desktop/mobile browser scenarios pass.
- 2026-07-18 — G9 done: Program Administrators now use the shared staff calendar with organization-
  wide cohort and active-mentor options, month/week/agenda views, full event logistics, cohort
  filtering, and admin attendance destinations. One-time and weekly session creation uses the
  existing server-authoritative term boundary and reports the exact generated count. Four adjacent
  scheduling/attendance integrations, six portal units, the production build, admin desktop/mobile
  acceptance, and the complete mentor calendar/attendance/export regression pass.
- 2026-07-18 — G10 done: Program Administrators can capture and correct attendance on behalf of staff
  from role-correct calendar/history links, including Present/Late/Excused/Absent marks, private
  notes, bulk actions, live tallies, optimistic concurrency, and the shared private offline queue.
  Organization-wide session and youth history preserves cohort/date filters, rates, trends, threshold
  flags, and exact-filter CSV/PDF downloads. Ten focused integrations, six portal units, the production
  build, and five combined admin/mentor desktop/mobile/offline browser scenarios pass.
- 2026-07-18 — G11 done: admins can send or schedule validated organization/cohort announcements to
  each supported role combination and review sent/scheduled history with exact recipient, read, and
  read-rate statistics. One shared targeting boundary now activates due schedules, resolves youth,
  guardian, mentor, volunteer, and admin cohort relationships, and rejects read writes from users
  outside the audience. Seven adjacent backend integrations, six portal units, the production build,
  and six combined admin/mobile/guardian/youth browser scenarios pass.
- 2026-07-18 — G12 done; H7 and H17 advanced: admins can apply one date/cohort/track/mentor scope to
  organization KPIs, attendance and cohort charts, staff load, and full youth/cohort reports with
  current record counts and generated timestamps. The detailed projections combine authoritative
  lesson, quiz, attendance, competency, check-in, badge, flag, enrollment, and staffing data, including
  program, selected-cohort, and previous-term comparisons without exposing private mentor-note text.
  Mentor endpoints remain limited to assigned youth and cohorts; true empty data, shared 500 recovery,
  and mobile reflow are covered. Nine reporting integrations, six portal units, the production build,
  and three desktop/mobile browser scenarios pass.
- 2026-07-18 — G13 and H17 done: the reports hub exports its current overview, generated youth,
  generated cohort, or staff view as a real CSV or valid PDF while preserving date/cohort/track/mentor
  scope and comparison selection. Mentor downloads reuse the same youth/cohort ownership checks, the
  legacy cohort path now rejects cross-mentor access, and audit-log CSV/PDF downloads apply actor,
  action, and date filters with private details redacted. Attendance and progress exports now join all
  new targets in writing durable filter-aware audit records; the shared CSV boundary neutralizes
  formula-like cells. Twenty-one adjacent backend integrations, six portal units, the production
  build, and seven report/progress/attendance desktop/mobile browser scenarios pass.
- 2026-07-18 — G14 done: admins can upload a CSV-only roster of at most 2 MB, see exact row/column
  counts, auto-map common headers, explicitly remap Name/Email/Cohort/Mentor/Track/Status/Consent or
  skip columns, and preview every row with Ready, Duplicate, Missing consent, or Invalid reasons and
  totals. Preview and commit share one classifier, so only confirmed Ready rows create youth accounts;
  named cohort/mentor/track associations and enrollments are applied, in-file/existing emails remain
  idempotent, and audit details preserve outcome counts. Four import integrations, six portal units,
  the production build, and three desktop/mobile/validation browser scenarios pass.
- 2026-07-18 — G15 done: the admin audit workspace now resolves actor names, exposes stable user and
  action choices, applies inclusive from/to dates before newest-first paging, and preserves source,
  target, public detail, and private/redacted context without exposing safeguarding notes. The current
  actor/action/date scope flows into the real audited CSV download; empty filters recover cleanly and
  the card-table controls remain touch usable without horizontal overflow. Twenty-six adjacent
  consent/reporting integrations, six portal units, the 423.57 kB production build, and two
  desktop/mobile browser scenarios pass.
- 2026-07-18 — H1 done; H19 advanced: authenticated browser activity now resets one exact 30-minute
  idle boundary; expiry captures safe non-password/file form controls with the owner and route,
  revokes the server session, uses a two-second disconnected fallback, clears the local session, and
  hard-transitions past feature leave guards to the session-expired state. Re-authentication returns
  the same user to the reflection editor and restores text, mood, optional response, and mentor-share
  choice; another user cannot consume that draft, while explicit sign-out still purges all Word Up
  storage. Eight portal units, twenty auth-library units, the 426.57 kB production build, and four
  combined sign-out/expiry/reflection browser journeys pass.
- 2026-07-18 — H2 done: one endpoint authorization policy now reuses the term-aware participation
  evaluator across every protected Youth learning controller, including all reads, autosaves, and
  completion/submission mutations. Missing, expired, wrong-term, or withdrawn consent returns a
  consent-specific 403 state before application handlers run; the portal safely captures an active
  form and transitions to consent-required, while re-consent immediately restores the same session.
  Thirty-six Consent/Learning integrations, eight portal units, the 426.84 kB production build, and
  eleven curriculum/lesson/quiz/assignment/coding/reflection/journey/progress browser journeys pass.
- 2026-07-18 — H4 done: assignment evidence now applies one reusable granular media policy at upload,
  submission, Youth/staff projection, and private-file retrieval. Group-only permission cannot grant
  an individual photo; current all-photo/video permission enables it, and a guardian's later media-
  only withdrawal hides the retained safeguarding record immediately without affecting the text
  submission or participation session. Sixty-six adjacent Consent/Learning/Guardian/Mentoring
  integrations, eight portal units, the 426.89 kB production build, and six combined Guardian,
  participation-consent, and assignment-media browser journeys pass.
- 2026-07-18 — H5 advanced: onboarding and profile settings no longer accept external photo URLs.
  They now validate JPG/PNG MIME, extension, size, and signature before storing one private owned
  binary; replacement deletes the prior blob, removal deletes the active blob, and bearer-authenticated
  serving is limited to self, linked guardian, assigned mentor, or administrator. Linked-minor reads
  and uploads also recheck the individual-photo grant, including media-only withdrawal. Six profile/
  onboarding integrations, eight portal units, the 426.89 kB production build, and five combined
  onboarding/profile/password browser journeys pass; assignment files and support screenshots remain.
- 2026-07-18 — H5 advanced: assignment authors can allow PDF evidence alongside text, link, and
  photo submissions. Youth uploads now enforce the 10 MB cap plus PDF MIME, extension, and magic-byte
  agreement; the selected private blob is linked to the immutable submitted copy while unused drafts
  are deleted. Bearer-authenticated downloads use no-sniff headers and are limited to the owner,
  assigned mentor, or administrator; Youth and mentor views fetch private photos/files as blobs, and
  external assignment-photo URLs are no longer accepted. The complete backend inventory passes as
  213 segmented integration tests plus four unit tests; all 51 frontend units, the 426.89 kB production
  build, and five assignment/mentor-review browser journeys pass. Support screenshots remain.
- 2026-07-18 — H5 done: the support form now accepts an optional locally previewed JPG/PNG screenshot
  only after MIME, extension, 5 MB, and magic-byte validation. Uploads are private owner-bound blobs;
  users can remove a draft, submitting links the selected file and deletes abandoned uploads, and a
  linked copy cannot be detached from its recorded request. Administrators can retrieve evidence only
  after submission, all serving is authenticated and no-sniff, and the support email identifies the
  private evidence route. Nineteen Platform integrations, all 51 frontend units, the 426.89 kB
  production build, and both support browser journeys pass.
- 2026-07-19 — H7 done: Youth and Guardian progress now reuse one role-neutral projection for approved
  quiz outcomes and each competency's latest assessed level. The Guardian view combines those safe
  histories with its existing attendance and explicitly shared check-ins, while pending attempts,
  answer content, and private mentor assessment notes stay excluded. Fifty-four adjacent Guardian,
  Learning, and Reporting integrations, all 51 frontend units, the 426.89 kB production build, and
  both populated and empty Guardian progress browser journeys pass.
- 2026-07-19 — H12 done: mentor grading drafts/final decisions and Youth resubmissions now read and
  echo a required application-managed Submission version. Accepted mutations advance the token;
  sequential stale edits and database concurrency races return 409 before side effects can overwrite
  the winning review. Mentor and Youth conflict banners preserve local form work and explicitly load
  the latest rubric or feedback for reconciliation before retry. Sixty-two Learning, Mentoring, and
  NonFunctional integrations, all 51 frontend units, the 426.89 kB production build, and all seven
  assignment/grading browser journeys pass.
- 2026-07-19 — H13 done: one preference-aware delivery service now prepares durable notifications in
  the caller's unit of work and sends enabled email/realtime channels only after persistence. Canonical
  `work-graded`, `badge-earned`, `mentor-messages`, `lesson-reminders`, `session-reminders`,
  `at-risk-alert`, and `approval-needed` producers cover grading, every automatic badge path, direct/
  broadcast/assignment messages, once-only scheduled delivery, new automatic flags, initial/revised
  submissions, and quiz attempts with role-correct deep links. The settings vocabulary exposes all
  keys. Fifty affected backend integrations, all 51 frontend units, the 426.89 kB production build,
  and the notification-settings browser acceptance pass.
- 2026-07-19 — H15 done: every authenticated 403 now records the actor, endpoint, IP location, and
  user agent from an isolated audit scope, covering both role-policy failures and resource/IDOR
  denials without retaining query strings or request bodies. The existing application approval, role
  change, lesson edit, attendance, check-in, badge award, sign-in, and account-deactivation producers
  flow through a sequenced HMAC-SHA256 chain; EF blocks audit updates/deletes, a unique sequence guards
  append order, and whole-chain verification exposes a prominent admin incident warning for legacy,
  missing, changed, or forged records. All 225 backend integrations plus four unit tests, all 51
  frontend units, the 426.89 kB production build, and both healthy/failed audit-integrity browser
  scenarios pass.
- 2026-07-19 — H16 done: a production-only transport gate now rejects non-HTTPS traffic before any
  credential or token handler runs while preserving the global security headers. CORS reads only the
  active configuration's exact origins, rejects malformed entries, requires HTTPS origins in
  Production, defaults Development to the two local web apps, and otherwise fails closed. A shared,
  configurable per-IP fixed window now covers all `/api/auth` routes (including reset), every
  invitation route, and staff-triggered password resets; rejected calls return 429 with Retry-After.
  Nine dedicated Development/Production host acceptances, twenty-one adjacent security/auth tests,
  all 234 backend integrations, and all four backend unit tests pass.
- 2026-07-19 — H18 done: self-service data-rights requests now receive a durable reference and move
  through a restricted administrator queue to one-time access, correction, removal, or consent-
  withdrawal fulfilment. Removal and configurable age-based retention share an idempotent anonymizer
  that revokes sessions, deletes private files/tokens, redacts authored and safeguarding content, and
  retains pseudonymous attendance/progress/outcome rows verified through the standard report aggregate.
  Sensitive workflow, messaging, mentoring, learning, attendance, support, and guardian free-text fields use
  versioned AES-GCM storage converters; legacy plaintext remains readable during migration and modified
  values are encrypted with authenticated tamper detection. All 239 backend integrations plus four unit
  tests, all 51 frontend units, the 426.89 kB portal and 339.30 kB public-site builds, and both portal
  smoke journeys pass.
- 2026-07-19 — H19 done: a shared dirty-state adapter now gives reflection, mentor/volunteer
  attendance, grading, badge, progression, and every curriculum sub-editor consistent route and
  browser-close protection. Confirmed navigation saves either the existing server draft or a
  synchronous owner-bound local draft; cancelling stays on the editor, return restores once, and
  another account cannot consume the data. Curriculum selection also ignores stale workspace
  responses so concurrent loads cannot overwrite the active editor. All 54 frontend units, the
  426.96 kB production portal build, and sixteen adjacent grading, authoring, reflection, and
  attendance browser scenarios pass.
