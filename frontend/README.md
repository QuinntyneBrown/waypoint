# Waypoint frontend

This Angular 21 workspace is layered into application, feature, API, and component projects.

| Layer              | Projects                                                                                                                                                                                                                 | May depend on                                                                                                         |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------- |
| Applications       | `portal`, `public-site`                                                                                                                                                                                                  | feature libraries, `@waypoint/api`, `@waypoint/components`                                                            |
| Feature libraries  | `@waypoint/auth` (+ `@waypoint/auth/pages`), `@waypoint/core` (+ `/realtime`, `/attendance`), `@waypoint/account`, `@waypoint/youth`, `@waypoint/mentor`, `@waypoint/admin`, `@waypoint/guardian`, `@waypoint/volunteer` | `@waypoint/api`, `@waypoint/components`, and the foundational feature libraries `@waypoint/auth` and `@waypoint/core` |
| API library        | `@waypoint/api` (`projects/api`)                                                                                                                                                                                         | nothing in the workspace                                                                                              |
| Components library | `@waypoint/components` (`projects/components`)                                                                                                                                                                           | nothing in the workspace                                                                                              |

- `@waypoint/api`: services that talk to the backend, plus the DTO and enum models.
- `@waypoint/components`: presentation components, the shell, icons, design-system styles, and UI-only services such as toasts and offline status.
- `@waypoint/auth`: session state, guards, the auth interceptor, and (in the `pages` entry point) the sign-in, invitation, and password-reset pages.
- `@waypoint/core`: portal-wide feature services: drafts, unsaved-changes guard, consent guard, idle session, interceptors, display preferences, and shell role. The SignalR `realtime` and offline `attendance` queue services are secondary entry points so the portal can lazy-load them.
- `@waypoint/account`: settings, onboarding, help, support, invitations, and the error/state pages.
- `@waypoint/youth`, `mentor`, `admin`, `guardian`, `volunteer`: each role's pages.

Role feature libraries never import each other; where one role reuses another role's page (for example the admin calendar), the portal's routes compose it. `npm run packages:check` enforces that the API and components libraries have no workspace dependencies and that nothing depends on a role feature library.

## Prerequisites

Use Node.js `^20.19.0`, `^22.12.0`, or `>=24.0.0` and npm 10.9.4. From this directory, install the locked dependencies with:

```bash
npm ci
```

## Local development

Run the API from one terminal:

```bash
npm run api
```

Run the portal at `http://localhost:4200` from another:

```bash
npm start
```

The public site can run concurrently at `http://localhost:4201`:

```bash
npm run start:public
```

Both applications proxy `/api` and `/hubs` to `http://127.0.0.1:5211`.

## Build and package verification

The root build is deliberately ordered: it builds the libraries in dependency order (`api`, `components`, `auth`, `core`, then the feature libraries), then the portal and public site, then verifies the generated package metadata, entry points, tarball contents, and cross-package imports.

```bash
npm run build
```

The narrower commands are available when needed:

```bash
npm run packages:build
npm run apps:build
npm run packages:check
```

`npm run packages:check` expects current library output under `dist/`; it performs an `npm pack --dry-run` for each package without publishing anything.

## Tests and quality gates

Angular unit tests use Vitest. A library gets a `test` target in `angular.json` once it has a spec (`youth`, `mentor`, `guardian`, and `volunteer` have none yet):

```bash
npm test
npx ng test portal
npx ng test public-site
npx ng test api
npx ng test components
npx ng test auth
npx ng test core
npx ng test account
npx ng test admin
```

Browser acceptance, responsive, accessibility, and Chromebook-class interaction gates use Playwright:

```bash
npx playwright install chromium
npm run e2e:ci
npm run e2e:performance
```

Run formatting and route-manifest checks with:

```bash
npm run format:check
npm run quality:routes
```

See the repository [README](../README.md) and [contribution guide](../CONTRIBUTING.md) for backend, SQL Server, traceability, and full release-gate instructions.
