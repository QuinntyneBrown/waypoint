# Waypoint — stakeholder demo plan

A repeatable script for showing Waypoint to someone who has **never seen it before**, in
either **15 minutes** (core) or **30 minutes** (extended).

This document covers three things:

1. **The narrative** — one story that carries the whole demo.
2. **The data setup** — exactly how to build the scenarios the story needs, from an empty database.
3. **The run sheet** — minute-by-minute, with what to click and what to say.

> Waypoint is a development project. Demo with **synthetic data only** — never real participant
> information. See [SECURITY.md](../SECURITY.md) and the current [implementation status](implementation-plan.md).

---

## 1. What this demo is for

**Audience:** a program director, funder, board member, or partner organization. Assume they know
youth programs and know nothing about software.

**The one thing they should walk away believing:**

> "Everything that used to live in a spreadsheet, a group chat, and a filing cabinet is now one
> system — and it knows who is allowed to see what."

**Three supporting points, in priority order:**

| # | Point | Proven by |
|---|-------|-----------|
| 1 | One young person's journey is visible end to end | Application → cohort → lesson → quiz → badge → report |
| 2 | Every role sees only their slice | Youth, mentor, guardian, volunteer, and admin screens side by side |
| 3 | Safeguarding is built in, not bolted on | Consent gate blocking access, audit trail, scoped data |

**Non-goals.** Do not demo the admin settings surface, the full curriculum authoring workspace, or
the API/Swagger — unless the stakeholder is technical and asks. Do not promise dates.

---

## 2. The narrative

Everything hangs off **one young person** and the adults around her. Use the same names all the way
through; never say "the user."

> **Amara is 18. She hears about Waypoint from a friend, applies on the website, gets accepted into
> the Fall cohort, works through her first leadership lesson, passes the quiz, and earns her first
> badge. Along the way, her mentor Grace is watching her progress, Grace's other youth David is
> falling behind and gets flagged, guardian Ruth has to grant consent before her daughter Imani can
> participate at all, volunteer Sam takes attendance at a Saturday session, and administrator Pat
> can see the whole cohort and export the report the funder asked for.**

That single paragraph is your opening. Say it before you touch the keyboard, then spend the rest of
the time walking it.

**Order of roles matters.** Public → Admin → Youth → Mentor → Guardian → Admin. It follows Amara
forward in time, and it ends on the screen a director cares about most (the progress board and the
report).

---

## 3. The cast — scenario data

Every persona exists to demonstrate exactly one thing. Keep the cast this small; more people makes
the story harder to follow, not more impressive.

| Persona | Role | Sign-in | Demonstrates |
|---|---|---|---|
| **Pat Morgan** | ProgramAdministrator | `pat@waypoint.demo` | Intake review, cohort oversight, reporting, audit |
| **Grace Adeyemi** | Mentor | `grace@waypoint.demo` | Mentor dashboard, check-ins, risk flags, grading |
| **Amara Bell** | Youth (on track) | `amara@waypoint.demo` | The happy path: lesson → quiz → badge → journey |
| **David Osei** | Youth (falling behind) | `david@waypoint.demo` | The at-risk case that mentoring is built to catch |
| **Imani Clarke** | Youth (consent-blocked) | `imani@waypoint.demo` | The safeguarding gate |
| **Ruth Clarke** | Guardian (Imani's) | `ruth@waypoint.demo` | Guardian visibility + granting consent |
| **Sam Whitfield** | Volunteer | `sam@waypoint.demo` | Narrow, session-scoped access |
| *8 roster rows* | Youth (bulk) | — | CSV import with a duplicate and a missing-consent row |

Shared demo password: `Demo!Pass123` — put it on a sticky note, not on screen.

**Cohort:** *Fall 2026 — Tuesdays* · Track: *Foundations of Leadership* · Lead mentor: Grace.

### The three youth states, and why each exists

- **Amara — on track.** She has completed a lesson and passed a quiz *before* the demo starts. Do
  not start her from zero: an empty progress screen shows nothing. Leave her one lesson short of
  finishing the module so you can complete it live and watch the badge land.
- **David — falling behind.** Enrolled, no lesson completions, a failed quiz attempt, two missed
  sessions, and an open risk flag from Grace. He is the reason the mentor screens exist.
- **Imani — consent-blocked.** Linked to guardian Ruth with **no consent record**. She can sign in
  and gets stopped at the consent gate. Ruth grants consent live, and Imani's access opens.

---

## 4. Setup

Budget **60 minutes the first time**, **15 minutes** once you have run it before.

### 4.1 Prerequisites

- .NET SDK 9 (pinned by [`global.json`](../global.json)), Node.js per [README](../README.md)
- `jq` and `curl` for the seed script
- **SQL Server** (Express, LocalDB, or Docker) — see 4.2
- A local SMTP catcher (smtp4dev, MailHog, or Papercut) — see 4.3

### 4.2 Use SQL Server, not the in-memory database

The default development database is **in-memory and process-local**: if the API restarts — a crash,
a stray `Ctrl+C`, a file-watch rebuild — **your entire demo dataset is gone**, mid-sentence. Use a
real database so the data survives and so you can re-run the demo without re-seeding.

```bash
export ConnectionStrings__Default='Server=localhost;Database=WaypointDemo;Trusted_Connection=True;TrustServerCertificate=True'
```

### 4.3 Run a mail catcher

Invitation links, password resets, and consent request links are **emailed**. With the shipped
defaults (`Smtp:Enabled = false`) the SMTP transport throws and **no link is ever retrievable** —
the default `LoggingEmailSender` logs only "Email queued through the configured delivery channel",
with no token. If you plan to show invitation acceptance, you need a catcher.

```bash
# e.g. smtp4dev on 2525, web UI on 5000
export Smtp__Enabled=true
export Smtp__Host=127.0.0.1
export Smtp__Port=2525
export Smtp__UseStartTls=false
export Smtp__FromEmail=noreply@waypoint.demo
```

If you would rather not run one: the seed script sets passwords directly, so **every persona can
sign in without an invitation email**. Skipping the catcher only costs you the invite-acceptance
beat (extended demo, §6.2).

### 4.4 Start the three processes

Three terminals, from the repo root, with the environment variables above exported in terminal 1:

```bash
# 1 — API
dotnet run --project backend/src/Waypoint.Api --environment Development --urls http://127.0.0.1:5211

# 2 — authenticated portal        → http://localhost:4200
npm --prefix frontend start

# 3 — public site                 → http://localhost:4201
npm --prefix frontend run start:public
```

### 4.5 Seed the skeleton

Save as `scripts/demo/seed-demo.sh`. It builds everything that cannot be created quickly by hand:
the operator account, a published curriculum, the staff accounts, and the cohort.

```bash
#!/usr/bin/env bash
set -euo pipefail

API=${API:-http://127.0.0.1:5211}
PW='Demo!Pass123'

# Build once and run with --no-build --verbosity quiet: otherwise MSBuild chatter lands on stdout
# and every `| jq -r .id` in this script parses garbage.
dotnet build backend/src/Waypoint.Cli -c Release -v q --nologo
CLI="dotnet run --project backend/src/Waypoint.Cli -c Release --no-build --verbosity quiet --"

# 1 — schema + first Program Administrator ---------------------------------
$CLI db migrate
$CLI db seed --email pat@waypoint.demo --password "$PW" --display-name 'Pat Morgan'

# 2 — admin bearer token (audited, no browser needed) ----------------------
TOKEN=$($CLI token issue --user pat@waypoint.demo | jq -r .accessToken)
AUTH=(-sS -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json')

# 3 — curriculum: track -> module -> lesson -> quiz ------------------------
# Youth only ever see PUBLISHED content, so every level is published at the end.
TRACK=$(curl "${AUTH[@]}" -X POST "$API/api/tracks" -d '{
  "name":"Foundations of Leadership","key":"leadership",
  "description":"Character, courage, and calling for emerging young leaders."}' | jq -r .id)

MODULE=$(curl "${AUTH[@]}" -X POST "$API/api/modules" -d "{
  \"trackId\":\"$TRACK\",\"name\":\"Called to Lead\",
  \"description\":\"Why your age is not a disqualification.\",
  \"scriptureAnchor\":\"1 Timothy 4:12\"}" | jq -r .id)

LESSON=$(curl "${AUTH[@]}" -X POST "$API/api/lessons" -d "{
  \"moduleId\":\"$MODULE\",\"title\":\"Don't let anyone look down on you\",
  \"scriptureReference\":\"1 Timothy 4:12\",\"estimatedMinutes\":20,
  \"learningObjectives\":\"Name one area where you can set an example this week.\",
  \"teachingBody\":\"Paul writes to a young leader who is being dismissed for his age...\",
  \"reflectionPrompts\":\"Where do you feel overlooked? What would leading anyway look like?\"}" | jq -r .id)

LESSON2=$(curl "${AUTH[@]}" -X POST "$API/api/lessons" -d "{
  \"moduleId\":\"$MODULE\",\"title\":\"Leading yourself first\",
  \"scriptureReference\":\"Proverbs 25:28\",\"estimatedMinutes\":20,
  \"teachingBody\":\"Self-leadership is the part nobody claps for...\"}" | jq -r .id)

# feedbackMode: show-correct | score-only | after-all-attempts
QUIZ=$(curl "${AUTH[@]}" -X POST "$API/api/quizzes" -d "{
  \"moduleId\":\"$MODULE\",\"lessonId\":\"$LESSON\",\"passMark\":70,\"attemptsAllowed\":3,
  \"feedbackMode\":\"show-correct\",\"shuffle\":false,\"requiredToCompleteModule\":true}" | jq -r .id)

# type: 1 MultipleChoice | 2 TrueFalse | 3 MultipleAnswer | 4 ShortAnswer  (numeric — not a string)
curl "${AUTH[@]}" -X POST "$API/api/quizzes/$QUIZ/questions" -d '{
  "text":"Who is Paul writing to in 1 Timothy 4:12?","type":1,
  "options":["Timothy","Titus","The church at Corinth","Barnabas"],"correctIndexes":[0]}' >/dev/null

curl "${AUTH[@]}" -X POST "$API/api/quizzes/$QUIZ/questions" -d '{
  "text":"Being young disqualifies you from leading.","type":2,
  "options":["True","False"],"correctIndexes":[1]}' >/dev/null

for id in "$LESSON" "$LESSON2"; do curl "${AUTH[@]}" -X POST "$API/api/lessons/$id/publish" >/dev/null; done
curl "${AUTH[@]}" -X POST "$API/api/modules/$MODULE/publish" >/dev/null
curl "${AUTH[@]}" -X POST "$API/api/tracks/$TRACK/publish"  >/dev/null

# 4 — staff accounts -------------------------------------------------------
MENTOR=$($CLI user create --email grace@waypoint.demo --password "$PW" \
  --display-name 'Grace Adeyemi' --role Mentor | jq -r .id)
GUARDIAN=$($CLI user create --email ruth@waypoint.demo --password "$PW" \
  --display-name 'Ruth Clarke' --role Guardian | jq -r .id)
$CLI user create --email sam@waypoint.demo  --password "$PW" \
  --display-name 'Sam Whitfield' --role Volunteer >/dev/null

# 5 — cohort (requires a PUBLISHED track and an active mentor) -------------
COHORT=$($CLI cohort create --name 'Fall 2026 — Tuesdays' \
  --track "$TRACK" --lead-mentor "$MENTOR" --location 'Community Hall' | jq -r .id)

# 6 — youth ----------------------------------------------------------------
$CLI user create --email amara@waypoint.demo --password "$PW" \
  --display-name 'Amara Bell'  --role Youth --cohort "$COHORT" --mentor "$MENTOR"
$CLI user create --email david@waypoint.demo --password "$PW" \
  --display-name 'David Osei'  --role Youth --cohort "$COHORT" --mentor "$MENTOR"
IMANI=$($CLI user create --email imani@waypoint.demo --password "$PW" \
  --display-name 'Imani Clarke' --role Youth --cohort "$COHORT" --mentor "$MENTOR" | jq -r .id)

# 7 — guardian linkage, deliberately WITHOUT a consent record --------------
# This is the consent gate: Imani has a guardian on file and no consent, so she is
# blocked until Ruth grants it live in §5. Audit-logged as GuardianLinked.
$CLI user link-guardian "$IMANI" "$GUARDIAN"

cat <<EOF

Seeded.
  track    $TRACK
  module   $MODULE
  cohort   $COHORT
  mentor   $MENTOR
  guardian $GUARDIAN -> youth $IMANI
Password for every persona: $PW
EOF
```

Substitute `way` for `dotnet run --project backend/src/Waypoint.Cli --` if you have installed the
packed tool (`PackAsTool`, command name `way`). It is not in the repo's tool manifest by default.

### 4.6 Build the scenario states through the UI

The script gives you a cohort of three youth who differ only in that Imani has a guardian on file.
The *interesting* states are the ones you create by using the product — which also serves as your
rehearsal. Do these in order, ~10 minutes:

| # | Sign in as | Do this | Creates |
|---|---|---|---|
| 1 | Amara | Complete **lesson 1**, take the quiz, **pass** it | The on-track youth with real progress |
| 2 | David | Open the quiz, answer wrong, **fail** it | The struggling youth with evidence |
| 3 | Grace | Open David → raise a **risk flag**, log a **check-in** | The mentor's early-warning trail |
| 4 | Grace | Schedule a **session** this week; mark David absent for a past one | Attendance history + something for Sam |
| 5 | Amara | Start (don't finish) **lesson 2** | A live finish during the demo → badge |

> **Guardian linkage.** The seed script already links Ruth to Imani with
> `way user link-guardian <youth-id> <guardian-id>` (§4.5, step 7) and records **no** consent — that
> is what arms the consent gate. Both accounts must be **active**, the youth must have the Youth
> role and the guardian the Guardian role, and the link is audit-logged as `GuardianLinked`, so it
> shows up in Pat's audit view alongside Ruth's consent in §5. To link someone else on the fly
> (`$CLI` as defined in §4.5):
>
> ```bash
> IMANI=$($CLI user list --search imani@waypoint.demo | jq -r '.items[0].id')
> RUTH=$($CLI user list --search ruth@waypoint.demo  | jq -r '.items[0].id')
> $CLI user link-guardian "$IMANI" "$RUTH"
> ```

Then prepare the roster CSV for the extended demo (headers are fixed: `Name, Email, Cohort, Mentor,
Track, Status, Consent`). Deliberately include **one duplicate email** and **one row with consent
missing** so the import summary reports something real:

```csv
Name,Email,Cohort,Mentor,Track,Status,Consent
Joy Mensah,joy@waypoint.demo,Fall 2026 — Tuesdays,Grace Adeyemi,Foundations of Leadership,Active,Yes
Marcus Reid,marcus@waypoint.demo,Fall 2026 — Tuesdays,Grace Adeyemi,Foundations of Leadership,Active,Yes
Nia Fofana,nia@waypoint.demo,Fall 2026 — Tuesdays,Grace Adeyemi,Foundations of Leadership,Active,No
Amara Bell,amara@waypoint.demo,Fall 2026 — Tuesdays,Grace Adeyemi,Foundations of Leadership,Active,Yes
```

Row 3 lands in **needs consent**; row 4 is a **duplicate**. Import returns
`{ imported, skippedDuplicates, needsConsent, invalid }` — that four-number summary *is* the beat.

### 4.7 Pre-flight checklist — run 10 minutes before

- [ ] All three processes up; `http://127.0.0.1:5211/health/ready` returns healthy
- [ ] Every persona signs in **and lands on their home screen** (do this in the browser you'll present in)
- [ ] Amara's progress screen shows real, non-zero progress
- [ ] Imani is blocked at the consent gate
- [ ] Grace's dashboard shows David's flag
- [ ] Five browser profiles/windows open and pre-signed-in, one per role — **switching accounts live
      is the single biggest time sink in this demo**
- [ ] Browser zoom ~125%, notifications silenced, bookmarks bar hidden
- [ ] Mail catcher open in a tab (if using)
- [ ] `docs/mocks/` closed — those are static references, not the app; showing them by accident
      undermines everything

---

## 5. Run sheet — 15-minute core

| Time | Beat | Role | Do | Say |
|---|---|---|---|---|
| 0:00–1:00 | **The front door** | Public | Public site → *Programs* → *Apply*. Submit an application for a fictional 18-year-old. | "This is what a young person or a parent sees. No login, no PDF form to print." |
| 1:00–3:00 | **The decision** | Pat | Admin → *Applications* → open it → assign cohort + mentor → **Approve**. | "It arrives here, not in someone's inbox. Approving creates the account and places them with a cohort and a mentor in one step." |
| 3:00–7:00 | **The young person** | Amara | Home → *Curriculum* → finish **lesson 2** → reflection → quiz → **pass** → badge → *My journey*. | "This is the whole reason the system exists. Scripture, teaching, a reflection in her own words, a check for understanding — and something to be proud of." |
| 7:00–10:00 | **The mentor** | Grace | Dashboard → **David's flag** → open David → check-in history → review Amara's submission. | "Grace supports eight young people. The system tells her *which one needs her this week* — she doesn't have to remember." |
| 10:00–12:00 | **The guardian** | Ruth → Imani | Imani signs in → **blocked** at consent gate. Switch to Ruth → grant consent. Back to Imani → **in**. | "Imani cannot take part until her mum says yes. That's not a policy in a binder — the software enforces it." |
| 12:00–14:00 | **The whole program** | Pat | Cohort progress board → export the cohort report → audit log. | "This is the funder report. It took about four seconds, and every sensitive action underneath it is logged with who did it and when." |
| 14:00–15:00 | **Close** | — | Stop sharing. | "One application, one young person, five people supporting her — one system. What would you want to see next?" |

**If you are running behind, cut in this order:** the audit log → the public application submission
(open a pre-submitted one instead) → the mentor's submission review. **Never cut** the youth
experience or the consent gate.

---

## 6. Extended 30-minute version

Run the 15-minute core **unchanged**, then add the modules below in this order. Stop when you hit
25 minutes and leave 5 for questions.

### 6.1 Volunteer — the narrowest door (2 min)
Sign in as Sam. He sees *only* his assigned session and its roster — no curriculum, no progress, no
other youth. Mark attendance.
> "Sam is a Saturday volunteer. He sees the eleven names in the room and nothing else. That's not a
> setting someone remembered to switch on — it's what his role *is*."

This is the strongest single argument for role-based access. If the stakeholder is safeguarding-
minded, promote it into the core demo.

### 6.2 Invitation and onboarding (2 min)
As Pat, invite a new mentor. Show the email arriving in the mail catcher, open the link, set a
password, land on onboarding. *(Requires the mail catcher from §4.3.)*

### 6.3 Bulk roster import (3 min)
Admin → *Import* → upload the CSV from §4.6. Land on the four-number summary.
> "Eight rows in. Six imported, one duplicate skipped, one held back because consent isn't on file.
> It won't quietly create a record it isn't allowed to have."

Or via CLI: `way import youth --from roster.csv`.

### 6.4 Communication (2 min)
Post an announcement as Pat; show it appear on Amara's home and in her notifications. Show the
mentor–youth message thread.

### 6.5 Curriculum authoring (3 min)
Admin → *Tracks* → open the module → add a lesson with scripture, objectives, teaching body, and
reflection prompts → **publish**. Then show it live on Amara's curriculum screen.
> "Your curriculum team writes it here. Nothing reaches a young person until someone publishes it."

### 6.6 Privacy and audit (2 min)
Admin → *Privacy*: data export and removal requests. Then the audit log filtered to consent actions.

### 6.7 The engineering story (2 min) — technical stakeholders only
- 88 in-scope requirements in [`docs/specs`](specs/README.md), each traced to a passing acceptance test
- CI gate refuses to build if a requirement loses its test, or a test loses its requirement
  (`node scripts/check-traceability.mjs --strict`)
- Accessibility (WCAG 2.1 AA), responsive XS–XL, and load gates run on every change

> "The reason I can say 'this works' rather than 'this demoed' is that every requirement on that
> list has a test attached, and the build fails if that link ever breaks."

---

## 7. Landmines and fallbacks

These are the things that will actually bite you. Read this section before your first run.

| Risk | Why it happens | What to do |
|---|---|---|
| **A minor's application cannot be approved** | Approval requires a recorded guardian consent, and there is no implemented path to record consent for an applicant who has no account yet. Applications accept ages **12–18**; only an **18-year-old** can be approved end to end. | Use **age 18** for the live intake beat. Demo the consent gate separately through Imani and Ruth (§5), where it works properly. |
| **Demo data vanishes** | The default database is in-memory and dies with the API process. | Use SQL Server (§4.2). |
| **Invitation link is unobtainable** | `Smtp:Enabled` is `false` by default and the transport throws; the fallback sender logs no token. | Run a mail catcher (§4.3), or skip the invite beat — every persona has a password. |
| **Cohort creation fails** | `way cohort create` requires a **published** track id and an **active mentor** id. | The seed script publishes track, module, and lessons before creating the cohort. Keep that order. |
| **Youth sees an empty curriculum** | Youth see only **published** modules and lessons, sourced from their cohort's track (plus any explicit track enrolments). | Confirm all three publish calls returned success and the youth has `--cohort` set. |
| **Quiz question rejected** | `type` is a numeric enum (`1`–`4`); string values are not accepted. `feedbackMode` must be exactly `show-correct`, `score-only`, or `after-all-attempts`. | Use the values in the seed script verbatim. |
| **Guardian link is refused** | `way user link-guardian` requires an **active Youth** and an **active Guardian** — it exits `2` with "Choose an active youth for guardian linkage." or "Choose an active guardian." | Check both accounts exist with the right role (`way user list --search <email>`) and are active. The ids are positional: **youth first, guardian second**. |
| **Imported youth cannot sign in** | Roster import creates youth **records** with no password. | That's correct behaviour — say so. Signing-in personas come from `way user create`. |
| **Account switching eats the clock** | Five roles, one browser. | Separate browser profiles or windows, all pre-signed-in (§4.7). |
| **Something breaks live** | It happens. | Say "that's a bug, and it's on the list" and move on. Have screenshots of the youth journey and the progress board in a folder as a hard fallback. Do **not** fall back to `docs/mocks/` — those are static references and a stakeholder will not know the difference until they ask a question you can't answer. |

---

## 8. Reset between runs

```bash
# Destructive — drops and recreates the demo database, then re-seeds from scratch.
dotnet run --project backend/src/Waypoint.Cli -- db reset --confirm RESET
./scripts/demo/seed-demo.sh
```

`db reset` refuses to run without the exact option `--confirm RESET`. Make sure
`ConnectionStrings__Default` still points at the **demo** database before you run it.

Then redo §4.6 (about 10 minutes). If you demo often, consider snapshotting the SQL database after
§4.6 and restoring it instead — that turns a 25-minute reset into about 30 seconds.

---

## 9. Questions you will be asked

**"Is this live? Can we start using it?"**
> No. It's a working system, not a released one — hosting, security and safeguarding reviews, and
> backup and recovery are still ahead. Everything you saw today is synthetic data, deliberately.

**"Where does the data live?"**
> A SQL Server database we control. Sensitive values are encrypted at rest, and every sensitive
> action is written to a tamper-evident audit log.

**"Can a mentor see youth who aren't theirs?"**
> No — and not because of a screen that hides it. The server refuses the request. Same for
> volunteers, guardians, and youth.

**"What about young people without a laptop?"**
> Every screen works on a phone, and it's tested at sizes from small phones to desktops.

**"How much is left to do?"**
> The product scope is built and tested — 88 requirements, each with a passing acceptance test. What
> remains is release work: hosting, independent security and safeguarding review, and your team's
> own acceptance. That's tracked in [implementation-plan.md](implementation-plan.md).

**"Can it do [thing you didn't show]?"**
> Check [`docs/specs`](specs/README.md) before promising. Explicitly excluded ideas are listed in
> [out-of-scope.md](specs/out-of-scope.md). "I don't know, let me check" is a fine answer.

---

## 10. Related documents

- [PRD.md](PRD.md) — what the product is for
- [specs/README.md](specs/README.md) — the 88 in-scope requirements
- [ARCHITECTURE.md](ARCHITECTURE.md) — how it is built
- [backend/src/Waypoint.Cli/README.md](../backend/src/Waypoint.Cli/README.md) — the `way` operational CLI
- [implementation-plan.md](implementation-plan.md) — current implementation status and release boundary
