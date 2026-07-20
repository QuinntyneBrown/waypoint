# @wu/auth

`@wu/auth` provides Word Up's Angular session state, persistence, HTTP interceptor, role navigation, guards, and authentication providers.

The built package imports `@wu/api` at runtime and declares the matching `@wu/api` version as a peer dependency. Consumers must install both packages together, along with the declared Angular peers.

From `frontend/`, build and test it with:

```bash
npm run packages:build
npx ng test wu-auth
```

The ordered package build creates `dist/wu-api` before `dist/wu-auth`. The package check then validates generated metadata, entry points, tarball contents, and the declared `@wu/auth -> @wu/api` dependency:

```bash
npm run packages:check
```

The repository does not publish packages automatically. Publishing requires an approved registry, version, and access policy; validate the built artifact before publishing `dist/wu-auth`.
