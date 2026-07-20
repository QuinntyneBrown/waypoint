# Frontend Structure — Angular Workspace

Implemented folder structure for the Word Up frontend (`frontend/`). The Angular 21 workspace contains the `portal` and `public-site` applications plus the `wu-ui`, `wu-api`, and `wu-auth` libraries described below (see [DECISIONS.md](DECISIONS.md) #9).

Related docs: [ARCHITECTURE.md](ARCHITECTURE.md) (system boundary), [implementation-plan.md](implementation-plan.md) (delivery phases), [contract.md](contract.md) (design system + mock contract), [`specs/`](specs/README.md) (L1/L2 requirements).

---

## Are there multiple apps in the workspace?

**Yes — two applications, plus shared libraries.**

| Project       | Type        | Purpose                                                                                                                                                                                         |
| ------------- | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `portal`      | Application | The single **authenticated** SPA for all signed-in roles: Youth, Mentor, Program Administrator, Guardian, Volunteer. Role areas are lazy-loaded route groups, not separate apps.                |
| `public-site` | Application | The **unauthenticated** public website: program information, youth application/referral intake, contact, privacy, donation handoff. Server-side rendered / prerendered for SEO and performance. |
| `wu-ui`       | Library     | The `wu-` design system as Angular components, plus design tokens and global styles.                                                                                                            |
| `wu-api`      | Library     | Typed API contracts (DTOs mirroring the backend records) and per-subsystem HTTP services for `/api/*`.                                                                                          |
| `wu-auth`     | Library     | Session state, JWT access-token handling, refresh rotation, HTTP interceptor, functional route guards, role directives.                                                                         |
| `wu-realtime` | Library     | SignalR `HubConnection` wrapper for near-real-time notifications and messages (L2-NFR-005).                                                                                                     |

### Why two apps (and not one, and not one per role)

- **Not one single app.** The public site has fundamentally different delivery requirements: SEO, fast first paint, and accessibility for anonymous visitors (Phase 4 exit criteria in the [implementation plan](implementation-plan.md)). It must not carry the authenticated shell, token logic, or role features in its bundle. It is prerendered/SSR; the portal is a plain SPA.
- **Not one app per role.** All signed-in roles share one session model (JWT + rotating refresh tokens), one design system, one notification/messaging surface, and one shell. Splitting per role would duplicate auth, shell, and realtime plumbing five times for no isolation benefit — authorization is **server-authoritative** ([DECISIONS.md](DECISIONS.md) #5), so app boundaries add no security. Role separation inside `portal` is achieved with lazy-loaded route groups, so a Guardian never downloads the admin bundle.

Sequencing per the implementation plan: `portal` is generated first (Phase 3, youth + mentor slices); `public-site` follows in Phase 4. Libraries are generated as soon as the first app needs them.

---

## Workspace root

```
frontend/
├── angular.json              # Workspace config; one entry per project below (prefix "wu" for all)
├── package.json              # npm scripts (ng serve/build/test), Angular 21.2.x, rxjs, tslib
├── package-lock.json         # Locked dependency graph (committed)
├── tsconfig.json             # Base TypeScript config; per-project tsconfigs extend it
├── .prettierrc               # Formatting rules
├── .editorconfig             # Editor defaults
├── .gitignore                # Ignores node_modules/, dist/, .angular/
├── README.md                 # CLI usage (serve, build, test)
├── e2e/                      # Planned: end-to-end tests (runner TBD; Playwright recommended)
│   ├── portal/               #   feature-foldered, each spec carries "// Traces to: L2-…"
│   └── public-site/
├── node_modules/             # (generated, not committed)
├── dist/                     # (generated, not committed) build output per project
├── .angular/                 # (generated, not committed) CLI cache
└── projects/                 # newProjectRoot — ALL apps and libraries live here
    ├── portal/
    ├── public-site/
    ├── wu-ui/
    ├── wu-api/
    ├── wu-auth/
    └── wu-realtime/
```

All filenames are lowercase kebab-case (repo convention). Projects are generated standalone (no NgModules), with signals and `inject()`, functional guards/interceptors, and Vitest via `ng test`. Unit tests are colocated `*.spec.ts` files next to the code they cover.

---

## `projects/portal/` — authenticated application

Serves every signed-in role. The URL space is grouped by role area; each area is one lazy `loadChildren` boundary.

```
projects/portal/
├── public/                       # Static assets served as-is: favicon, fonts, app icons, images
├── src/
│   ├── index.html                # SPA host page
│   ├── main.ts                   # bootstrapApplication(App, appConfig)
│   ├── styles.css                # Global styles: imports wu-ui design tokens; app-level resets only
│   └── app/
│       ├── app.ts                # Root component: <router-outlet> only
│       ├── app.html
│       ├── app.css
│       ├── app.config.ts         # Providers: router, HttpClient + wu-auth interceptor,
│       │                         #   wu-realtime connection, error handling
│       ├── app.routes.ts         # Top-level routes: /auth, /onboarding, and one lazy
│       │                         #   route group per role area below
│       ├── shell/                # Authenticated layout: top bar, role-aware nav, notification
│       │                         #   drawer, messages entry point. Renders what the server says
│       │                         #   the user may see — never the authority itself.
│       ├── auth/                 # Sign-in, invitation acceptance, password reset request/confirm,
│       │                         #   lockout screens (mocks: docs/mocks/authentication/auth/; specs: AUTH)
│       ├── onboarding/           # Resumable onboarding + profile setup wizard (specs: AUTH/PLATFORM)
│       ├── features/
│       │   ├── youth/            # Home, journey, curriculum, lessons, Bible reflections, quizzes,
│       │   │                     #   coding projects, submissions, progress, badges, streaks,
│       │   │                     #   schedule, announcements, messages
│       │   │                     #   (mocks: youth/; specs: LEARN, ENGAGE)
│       │   ├── mentor/           # Dashboard, roster, youth detail, assignments, grading,
│       │   │                     #   competency, check-ins, flags, reviews
│       │   │                     #   (mocks: mentor/; specs: MENTOR)
│       │   ├── admin/            # People & roles, cohorts, mentor assignment, application
│       │   │                     #   review (approve/decline), data import/export, calendar &
│       │   │                     #   events, announcements, curriculum authoring, reports,
│       │   │                     #   audit log, org settings
│       │   │                     #   (mocks: admin/; specs: ADMIN, CURRIC, INTAKE, REPORT, ENGAGE)
│       │   ├── guardian/         # Youth progress view, announcements, consent capture &
│       │   │                     #   withdrawal (mocks: guardian/; specs: GUARDIAN, CONSENT)
│       │   └── volunteer/        # Schedule and attendance capture (incl. offline attendance
│       │   │                     #   sync, L2-NFR-004) within a narrow permission boundary
│       │                         #   (mocks: volunteer/; specs: VOLUNTEER, ENGAGE)
│       └── states/               # Shared empty / loading / error / forbidden / offline state
│                                 #   components (mocks: docs/mocks/platform-services/states/)
├── tsconfig.app.json
└── tsconfig.spec.json
```

**Inside each `features/<area>/`:** one `<area>.routes.ts` (the lazy entry), then one folder per screen/flow containing the page component (`.ts`/`.html`/`.css` + colocated `.spec.ts`). Data access goes through `wu-api` services — feature folders contain **no raw `HttpClient` calls and no duplicated DTOs**. Cross-cutting UI belongs in `wu-ui`; anything used by two or more areas must not live inside a single area's folder.

**Guard rule (non-negotiable):** route guards and hidden nav items in `portal` are presentational conveniences. The API response is the authority for access, consent gating, and redaction — the client must render forbidden/gated/redacted responses faithfully ([DECISIONS.md](DECISIONS.md) #5).

---

## `projects/public-site/` — unauthenticated application

SSR/prerendered for SEO, performance, and anonymous accessibility. Contains **no** auth session logic and never imports `wu-auth` or `wu-realtime`.

```
projects/public-site/
├── public/                       # Static assets: favicon, og-images, fonts
├── src/
│   ├── index.html
│   ├── main.ts                   # Browser bootstrap
│   ├── main.server.ts            # SSR bootstrap
│   ├── server.ts                 # SSR entry / prerender host
│   ├── styles.css                # Imports wu-ui tokens
│   └── app/
│       ├── app.ts / app.html / app.css
│       ├── app.config.ts         # + app.config.server.ts for SSR providers
│       ├── app.routes.ts
│       └── features/
│           ├── home/             # Landing + program information (specs: PUB; cohort status
│           │                     #   rendered from its single authoritative source, L2-PUB-002)
│           ├── apply/            # Public youth application / referral intake form — submits to
│           │                     #   the anonymous intake endpoint (specs: INTAKE; mocks: public/)
│           ├── donate/           # Handoff to the external PCI-compliant donation provider —
│           │                     #   no card data touches this app (L2-PUB-003)
│           ├── contact/          # Contact form / program contact details
│           └── privacy/          # Privacy, terms, safeguarding commitments
├── tsconfig.app.json
└── tsconfig.spec.json
```

---

## Shared libraries

Each library follows the standard CLI layout: `src/public-api.ts` (the only import surface), `src/lib/**`, `ng-package.json`, `tsconfig.lib.json`. Apps import via the TS path alias (e.g. `@wu/ui`), never deep-relative paths.

### `projects/wu-ui/` — design system

```
projects/wu-ui/src/lib/
├── styles/                       # Design tokens, typography, spacing — the wu- CSS ported from
│                                 #   docs/mocks (styleguide.html is the visual reference)
├── button/  card/  badge/  ...   # One folder per wu- component (wu-button, wu-card, …),
│                                 #   presentation-only: inputs/outputs, no HTTP, no routing
└── states/                       # Reusable empty/loading/error primitives shared by both apps
```

The `wu-` class contract in [contract.md](contract.md) governs markup and class names; the mocks under `docs/mocks/` remain the interaction reference ([DECISIONS.md](DECISIONS.md) #6). Display preferences (text size, contrast, motion, reading font — no theme toggle in v1) live here.

### `projects/wu-api/` — typed API client

```
projects/wu-api/src/lib/
├── models/                       # Request/response interfaces mirroring the backend's
│   │                             #   command/query records, foldered per subsystem
│   ├── auth/  intake/  consent/  invitations/  onboarding/  …
├── services/                     # One injectable service per subsystem wrapping /api/* calls
│   ├── auth-api.ts  intake-api.ts  consent-api.ts  …
└── errors/                       # Typed problem/validation error shapes and mapping helpers
```

The only place `HttpClient` is used for REST calls. When the backend contract changes, it changes here first — both apps consume this library, keeping DTOs single-sourced.

### `projects/wu-auth/` — session & access

```
projects/wu-auth/src/lib/
├── session-store.ts              # Signal-based session state (current user, roles, expiry)
├── token-refresh.ts              # Rotating refresh-token flow; handles family revocation on reuse
├── auth-interceptor.ts           # Attaches bearer token; drives refresh-on-401
├── guards/                       # authGuard, roleGuard — presentational routing only
└── directives/                   # e.g. *wuIfRole — show/hide by server-declared role
```

### `projects/wu-realtime/` — live updates

```
projects/wu-realtime/src/lib/
├── connection.ts                 # SignalR HubConnection lifecycle (connect on sign-in,
│                                 #   reconnect/backoff, dispose on sign-out)
└── channels/                     # Typed notification + message streams (L2-NFR-005) consumed
                                  #   by the portal shell and feature areas
```

---

## Mock → project mapping

| `docs/mocks/`                | Lands in                                                                    |
| ---------------------------- | --------------------------------------------------------------------------- |
| `application-intake/`        | `portal` → application-intake feature                                       |
| `authentication/`            | `portal` → `app/auth/` and access administration                            |
| `consent-and-safeguarding/`  | `portal` → consent gates and safeguarding administration                    |
| `curriculum-authoring/`      | `portal` → curriculum-authoring features                                    |
| `engagement/`                | `portal` → calendar, attendance, announcements, messages, and notifications |
| `guardian-portal/`           | `portal` → guardian features                                                |
| `learning-experience/`       | `portal` → youth learning features                                          |
| `mentoring/`                 | `portal` → mentor features                                                  |
| `non-functional/`            | Cross-cutting quality references                                            |
| `platform-services/`         | `wu-ui` state primitives + `portal` settings, help, and system states       |
| `program-administration/`    | `portal` → people, cohort, and organization features                        |
| `public-site/`               | `public-site` features                                                      |
| `reporting-and-data/`        | `portal` → reporting and import features                                    |
| `volunteer-portal/`          | `portal` → volunteer features                                               |
| `assets/`, `styleguide.html` | `wu-ui` styles/tokens (visual reference)                                    |

---

## Generation commands (when each phase begins)

```bash
cd frontend
ng generate application portal --style=css --prefix=wu           # Phase 3
ng generate library wu-ui --prefix=wu                            # with first portal slice
ng generate library wu-api --prefix=wu
ng generate library wu-auth --prefix=wu
ng generate library wu-realtime --prefix=wu                      # with notifications slice
ng generate application public-site --style=css --prefix=wu --ssr # Phase 4
```

Until the first target exists, `ng build`/`ng serve`/`ng test` fail by design and are **not** repository quality gates ([DECISIONS.md](DECISIONS.md) #7).

## Testing & ATDD

The moment behavioural logic lands in this workspace, the repository-wide ATDD loop applies (see [CLAUDE.md](../CLAUDE.md)): failing test first, seen to fail, minimum implementation, full suite green. Unit specs (Vitest) are colocated; acceptance-level frontend coverage lives in `frontend/e2e/`, feature-foldered, and every acceptance spec carries a `// Traces to: L2-<AREA>-NNN` comment resolvable by `scripts/check-traceability.mjs`. Server-authoritative behaviour (authorization, consent gating, redaction) is proven by **backend** acceptance tests; frontend e2e proves the client renders those outcomes correctly, plus the responsive, keyboard, focus, reduced-motion, contrast, empty, loading, error, and offline states required by Phase 3 exit criteria.
