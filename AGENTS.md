## Project Overview

This repository is `waypoint`, a youth leadership platform.
It is built with a .NET 9 backend (`backend/`) and an Angular 21 frontend (`frontend/`).

## Commands

Run from the repository root unless noted.

- Restore: `dotnet tool restore && dotnet restore backend/Waypoint.sln`
- Build: `dotnet build backend/Waypoint.sln --no-restore --configuration Release`
- Test: `dotnet test backend/Waypoint.sln --no-build --configuration Release --logger "trx;LogFileName=test.trx"`
- Traceability: `node scripts/check-traceability.mjs --strict`
- Frontend (from `frontend/`): `npm ci`, `npm run build`, `npm test`, `npm run e2e:ci`
- Lint (from `frontend/`): `npm run format:check`, `npm run lint`
- Storybook (from `frontend/`): `npm run storybook`, `npm run build-storybook`

## Project Structure

- `.agents/`
- `.agents/skills/`
- `.claude/`
- `.claude/skills/`
- `.config/`
- `.github/`
- `.github/workflows/`
- `backend/`
- `backend/src/`
- `backend/tests/`
- `docs/`
- `docs/design-system/`
- `docs/detailed-designs/`
- `docs/estimates/`
- `docs/mocks/`
- `docs/specs/`
- `frontend/`
- `frontend/e2e/`
- `frontend/projects/`
- `frontend/scripts/`
- `scripts/`
- `scripts/performance/`

## Naming Conventions

- Frontend API services are named `<Name>Service` in `<name>.service.ts` (for example `AuditService` in `audit.service.ts`), never `<Name>Api` or `<name>-api.ts`.
- Components are named `<Name>` in `<name>.ts` (for example `Alert` in `alert.ts`), never with a `Ph`
  class prefix or a `ph-` file prefix. Selectors (`<ph-alert>`, `button[ph-button]`, `[phAccessibleDialog]`),
  `.ph-*` CSS classes and `--ph-*` custom properties use the `ph` prefix (DECISIONS #25); ESLint enforces it.

## Component Files (mandatory)

- Each component has its own `<name>.ts`, `<name>.html` and `<name>.css`, referenced with
  `templateUrl` and `styleUrl`: one component per file set, no inline `template` or `styles`, no
  stylesheet shared between components. ESLint (`@angular-eslint/component-max-inline-declarations`)
  enforces the no-inline part.

## Testing

- Backend tests use NUnit. Run them with `dotnet test backend/Waypoint.sln --no-build --configuration Release --logger "trx;LogFileName=test.trx"`.
- Integration tests use SQL Server: `(localdb)\MSSQLLocalDB` by default, or set `WAYPOINT_TEST_SQLSERVER` and `WAYPOINT_TEST_SQLAUTH`.
- Frontend unit tests use Vitest (`npm test` from `frontend/`).
- Frontend acceptance tests use Playwright (`npm run e2e:ci` from `frontend/`).

## Git Workflow

- Every change is checked by continuous integration:
  - `.github/workflows/backend.yml`
  - `.github/workflows/frontend.yml`
  - `.github/workflows/performance.yml`

## Components library is mandatory (CRITICAL)

- Every application and feature library MUST be built from `@waypoint/components`
  (DECISIONS #23). Never hand-write markup or CSS for a design-system component
  the library provides; use the library component. Component `.ph-*` classes are
  not public API. Layout, spacing and typography utilities (`ph-stack`,
  `ph-cluster`, `ph-grid`, `ph-container`, `ph-page*`, `ph-mb-*`, `ph-mt-*`, `ph-muted`,
  `ph-eyebrow`, `ph-text-*`) remain shared CSS.
- The look comes from the PromiseHub design system copied into `docs/design-system/`
  (DECISIONS #25); never load anything from the promise-hub folder at build or run time.
- `docs/screen-composition.md` is binding. Before implementing or changing any
  screen, read its entry and use the components it lists. Build locally only the
  UI it marks as local.
- If the library lacks a component the screen needs, STOP. Add it to the
  library first. Do not work around it in the app.
- Keep `docs/screen-composition.md` aligned with the mocks.

## Scope, Simplicity, and Zero-Waste Engineering (CRITICAL)

This is the absolute highest-priority rule in this repository. AI agents tend to over-engineer, hallucinate future needs, and balloon codebase complexity. This kills project velocity and comprehensibility. You must resist this impulse. Deliver radically simple solutions that fulfill the exact prompt—nothing more.

### 1. Absolute Scope Lock
*   **Deliver Only the Explicit Request:** Implement exactly what is asked for in the immediate task. Do not invent features, add unrequested states, create predictive API endpoints, or spin up new UI screens/components unless explicitly ordered.
*   **Zero Future-Proofing:** Do not write code for "imagined future use cases." Write code strictly for the present moment. If the user didn't ask for it today, it does not exist.
*   **Ignore Unsent Requirements:** Documents, specs, or mocks may contain broader context or future phases. Do not implement them early. Only implement what is requested in the active prompt.

### 2. Radical Simplicity & Proportionality
*   **Proportional Diffs:** The size and footprint of your code change must match the size of the request. A small feature or fix must result in a small, clean, readable diff.
*   **Path of Least Resistance:** Always choose the simplest, most direct code path that solves the problem. Do not introduce new abstractions, architectural layers, design patterns, wrappers, or external libraries unless it is physically impossible to solve the issue without them.
*   **No Unsolicited Refactoring:** Do not "improve," rewrite, or clean up code adjacent to your task unless it is strictly broken and blocking your implementation. Leave surrounding code alone.

### 3. Hard Stop & Query Clause
*   **When in Doubt, Ask:** If a task seems to logically require expanding the scope, introduces a major jump in complexity, or conflicts with the existing architecture, **STOP IMMEDIATELY**. Do not guess. Present the trade-offs to the user and await explicit confirmation before writing a single line of code.

## Incremental Implementation and ATDD - mandatory

Mocks and the design system are design artifacts. ATDD does not apply to their
development. Do not write tests for mocks or the design system.

Every new feature or change to production behavior MUST have a requirement, a
detailed design, and a mock before implementation begins. This includes
behavioral changes to existing features, such as changing how a page behaves.
This requirement applies to production-code changes only; documentation-only,
design-system-only, mock-only, and test-only changes are out of scope unless
they are part of implementing a production behavior change.

Every production behavior implementation MUST invoke and follow the
`implementing-incrementally` skill (`.claude/skills/implementing-incrementally`
and `.agents/skills/implementing-incrementally`) before any code is written,
combined with acceptance test-driven development (ATDD). Plan small,
reviewable slices, then complete one slice at a time: write Given-When-Then
acceptance criteria, write the acceptance test, and run it to prove it fails for
the expected reason BEFORE writing production code. Implement only what satisfies
that slice, refactor with tests green, and run the relevant regression checks.
Do not move to the next slice until those checks pass. No bulk implementation,
no tests added afterward, and no weakening tests to manufacture a pass. Keep
the requirement, detailed design, mock, criteria, tests, and implementation
aligned until the entire feature or behavior change is complete.

Front end tests use Playwright with the Page Object Model - one page object per
screen, owning the selectors and the interactions. Tests state intent; page
objects know the DOM. Never put a selector in a new or changed test. Existing
specs that still hold selectors are moved into page objects when they are next
changed (DECISIONS #15).

Frontend unit tests (Vitest) never need the backend. Playwright acceptance tests
run against the real API that `playwright.config.ts` starts, seeded through the
Testing-only endpoints (DECISIONS #9, #15); do not add route interception for
flows that already run against the real API.

Run frontend tests in Chromium only. Do not configure or run Firefox, WebKit,
or any other browser for frontend testing.

### Never write architecture tests

Never add a test that asserts the shape of the codebase rather than its behavior:
no structure, layout, or naming tests; no banned-API scans; no traceability tests
that parse the specifications. The one exception is the requirement traceability
gate, `scripts/check-traceability.mjs --strict`, which CI runs (DECISIONS #1, #15);
do not add other gates like it. Those constraints belong to the compiler, the
formatter, and review. A test suite exists to prove behavior.

## Skills

Project skills live in `.claude/skills/` and are mirrored in `.agents/skills/`
for other agents. Keep the two copies identical.

- `engineering-requirements` — L1/L2 requirements with acceptance criteria
- `writing-software-design-documents` — detailed designs from requirements
- `writing-html-mocks` — static HTML mocks for every page, dialog and
  notification in every state
- `extracting-design-systems` — design system from the HTML mocks
- `writing-component-requirements-documents` — CRDs for UI components
- `writing-screen-composition` — screen-to-component map and the library mandate
- `implementing-incrementally` — thin, verifiable slices (mandatory, see above)
- `creating-videos` — narrated walkthrough videos
- `estimating-designs` — per-subsystem effort and percent-complete tables


