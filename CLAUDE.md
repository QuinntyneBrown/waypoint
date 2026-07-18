# Word Up — Project Guide

Guidance for anyone (human or agent) working in this repo. Read this first.

## What this repo is
- **Front-end prototype** (`docs/mocks/`): a static, **offline** HTML mockup of the Word Up youth-leadership app, styled with FaithTech's design system. Runs from `file://`, zero network requests. Open `docs/mocks/index.html`. Design/markup contract: `docs/contract.md`.
- **Backend** (`backend/`, planned): a .NET 8 Clean Architecture API + `WordUp.Cli` dotnet tool (`wu`). Full spec: `docs/backend-implementation-plan.md`.

---

## Development workflow — ATDD is mandatory

**All code development in this repository shall be done using ATDD (Acceptance Test Driven Development).** No production code is written without a failing test that requires it.

### The loop — follow it for every change
1. **Write a failing integration test first.** Express the desired behaviour/logic as an integration test that drives the real seam (the HTTP API endpoint / the public surface), not internal classes. This is the acceptance criterion, written as executable code — **derived directly from an L2 requirement's acceptance criteria** (author the L2 requirement in `docs/specs/` first if it doesn't exist yet).
2. **Run it and watch it fail** — for the *right reason* (it asserts the missing behaviour, not a compile error, typo, or bad setup). A test that was never seen to fail proves nothing.
3. **Write the minimum implementation** needed to make that test pass — nothing more.
4. **Run the full suite — everything green.**
5. **Refactor** with the suite green, then move on.

**Integration test for the logic, then implementation to make the test pass.** That order is not optional.

### Rules
- **No production logic before a failing test.** If you catch yourself writing implementation first, stop and write the test.
- **The integration test is the driver** for behaviour/logic. Unit tests are welcome and encouraged to pin down pure logic (validators, domain rules, scoring, token issuance), but the *acceptance* of a feature is proven by an integration test through the real boundary.
- **Never weaken, skip, or delete a test to get to green.** If a test fails, fix the code (or fix the test only if the test itself is wrong — and say so explicitly).
- **One behaviour at a time.** Small red → green → refactor cycles, not big-bang implementations.
- **Commit only with the full suite passing.**

### How this maps to the backend (see `docs/backend-implementation-plan.md` §11)
- **Framework:** NUnit (unit + integration).
- **Integration tests** run against the real API via `WebApplicationFactory<Program>` with a **real SQL Server / SQL Express database, one throwaway DB per fixture** (`[OneTimeSetUp]` migrates, `[OneTimeTearDown]` drops). Drive behaviour through `HttpClient`; assert on responses and persisted state. SignalR behaviour is asserted via a real `HubConnection`.
- Tests are **feature-foldered** to mirror the app, named as `snake_case_sentences`.
- **Every acceptance test must trace to an L2 requirement.** Tag each acceptance test with the L2 id it verifies — a `// Traces to: L2-<AREA>-NNN` comment **and** a machine-readable `[Property("Req", "L2-<AREA>-NNN")]` attribute — where `L2-*` ids live in `docs/specs/`. No acceptance test without a governing L2 requirement; no L2 requirement is done without a passing acceptance test. A CI gate enforces this both ways (see `docs/backend-implementation-plan.md` §11).
- A new feature is "done" only when its integration test (written first, seen to fail) is green, it traces to its L2 requirement, and the full suite passes.

> Scope note: the front-end in `docs/mocks/` is **static mockups** (HTML/CSS + presentation-only JS) with no business logic — there is nothing to ATDD there beyond the offline/no-network invariants already enforced (`docs/contract.md`). The moment real logic is introduced (the backend, or any behavioural JS), the ATDD loop above is required.

---

## Conventions (summary — full detail in the docs)
- **Backend:** Clean Architecture (`WordUp.{Domain,Application,Infrastructure,Api}` + `WordUp.Cli`), file-per-type, folder-per-feature, thin controllers over MediatR **12.5.0** (the free Apache-2.0 pin), FluentValidation pipeline, EF Core + SQL Server, JWT OAuth2, SignalR via an `IChangePublisher` port. Records for commands/queries/DTOs; classes for handlers.
- **Front-end:** offline `file://`, no `fetch`/modules/external requests, lowercase filenames, `wu-` design-system classes only.

## Key docs
- `docs/backend-implementation-plan.md` — backend architecture, domain model, milestones.
- `docs/contract.md` — front-end component/class + JS API contract.
