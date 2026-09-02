# @w/api

`@w/api` is Waypoint's typed Angular API client. It exposes request and response models, API services, base-URL configuration, and normalized API errors for the portal and public site.

From `frontend/`, build and test it with:

```bash
npx ng build w-api
npx ng test w-api
```

The build output is `dist/w-api`. The workspace-wide package check validates its publishable name, generated entry points, and `npm pack --dry-run` contents:

```bash
npm run packages:build
npm run packages:check
```

The repository does not publish packages automatically. Publishing requires an approved registry, version, and access policy; validate the built artifact before publishing `dist/w-api`.
