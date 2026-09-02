# @w/ui

`@w/ui` is Waypoint's shared Angular design system. It contains the responsive application shell, icons, avatars, dialogs, state pages, toast and offline services, plus shared accessibility and visual styles.

From `frontend/`, build and test it with:

```bash
npx ng build w-ui
npx ng test w-ui
```

The build output is `dist/w-ui`. The workspace-wide package check validates its publishable name, generated entry points, and `npm pack --dry-run` contents:

```bash
npm run packages:build
npm run packages:check
```

The repository does not publish packages automatically. Publishing requires an approved registry, version, and access policy; validate the built artifact before publishing `dist/w-ui`.
