# @w/auth

`@w/auth` provides Waypoint's Angular session state, persistence, HTTP interceptor, role navigation, guards, and authentication providers.

The built package imports `@w/api` at runtime and declares the matching `@w/api` version as a peer dependency. Consumers must install both packages together, along with the declared Angular peers.

From `frontend/`, build and test it with:

```bash
npm run packages:build
npx ng test w-auth
```

The ordered package build creates `dist/w-api` before `dist/w-auth`. The package check then validates generated metadata, entry points, tarball contents, and the declared `@w/auth -> @w/api` dependency:

```bash
npm run packages:check
```

The repository does not publish packages automatically. Publishing requires an approved registry, version, and access policy; validate the built artifact before publishing `dist/w-auth`.
