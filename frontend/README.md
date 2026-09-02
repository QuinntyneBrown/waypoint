# Waypoint frontend

This Angular 21 workspace contains two applications and three shared libraries:

- `portal`: the authenticated role-aware application
- `public-site`: the SSR public site and intake experience
- `@w/api`: typed API models, services, and error handling
- `@w/auth`: session state, guards, and authentication integration
- `@w/ui`: the shared design system and application shell

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

The root build is deliberately ordered: it builds `@w/api`, `@w/auth`, and `@w/ui`, then the portal and public site, then verifies the generated package metadata, entry points, tarball contents, and cross-package imports.

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

Angular unit tests use Vitest:

```bash
npm test
npx ng test portal
npx ng test public-site
npx ng test w-api
npx ng test w-auth
npx ng test w-ui
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
