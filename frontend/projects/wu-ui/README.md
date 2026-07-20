# @wu/ui

`@wu/ui` is Word Up's shared Angular design system. It contains the responsive application shell, icons, avatars, dialogs, state pages, toast and offline services, plus shared accessibility and visual styles.

From `frontend/`, build and test it with:

```bash
npx ng build wu-ui
npx ng test wu-ui
```

The build output is `dist/wu-ui`. The workspace-wide package check validates its publishable name, generated entry points, and `npm pack --dry-run` contents:

```bash
npm run packages:build
npm run packages:check
```

The repository does not publish packages automatically. Publishing requires an approved registry, version, and access policy; validate the built artifact before publishing `dist/wu-ui`.
