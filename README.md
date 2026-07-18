# Word Up

A clickable **HTML mockup prototype** for **Word Up** — a program that trains youth (from Toronto subsidized housing across Rexdale, Warden, and St. Clair) to become *leaders who love Jesus* through Bible study, leadership training, and a coding track. It gives the program director a way to **monitor and track each youth's progression**, replacing the current manual Google Sheets.

Styled with **FaithTech's design system**.

> These are **mockups** — a static, front-end-only prototype for review and stakeholder demos. There is no backend.

## Open it

The mockups live in **`docs/mocks/`**. Just **double-click `docs/mocks/index.html`** — it's the prototype map that links to every screen. No server, no install, no internet required. Runs directly from the file system on any device (built to work on the org's shared Chromebooks, offline).

- `docs/mocks/index.html` — clickable sitemap of every screen (filter by role, search, open dialog/empty/loading variants)
- `docs/mocks/styleguide.html` — the living FaithTech design system

## What's inside (~115 screens)

| Folder | Screens | |
|---|---|---|
| `public/` | 8 | Landing, apply/refer, donate, about, privacy |
| `auth/` | 12 | Login, PIN sign-in, invite, guardian consent, shared-device sign-out & switch-user |
| `youth/` | 23 | Home, journey roadmap, Bible-lesson reader, quiz flow, reflections, badges, coding track |
| `mentor/` | 19 | Dashboard, roster, youth detail, competency tracker, check-ins, reviews, attendance |
| `admin/` | 29 | Director dashboard, **cohort progress board**, reports, curriculum builder, people & roles |
| `guardian/` · `volunteer/` | 7 | Read-only progress, consent, scoped attendance |
| `shared/` · `states/` | 17 | Settings, help, 404/403/500, offline, session-expired, consent gates |

Dialogs, empty states, and loading skeletons live on their parent page and are reachable from the hub via `?open=…` / `?state=…` URL params.

## How it's built (constraints)

The organization has only a handful of shared Chromebooks, so the prototype is deliberately dependency-free and offline-first:

- **Zero network requests** — the FaithTech fonts (Space Grotesk, Inter, Fraunces) are base64-embedded in the CSS; icons are inline SVG; avatars are generated from initials.
- **No build step, no framework, no npm.** Plain HTML + one CSS file + one classic JS file.
- Runs from `file://`, lowercase filenames, one level of folder nesting.

### Project layout
```
docs/
  mocks/                    # the prototype (all mock screens + assets)
    index.html              # prototype map / sitemap
    styleguide.html         # design system reference
    assets/css/wordup.css   # design tokens + components (built from _core.css + _fonts.css)
    assets/js/wordup.js     # shared shell, dialogs, toasts, icons, mock data (window.WordUp)
    _template-*.html        # copy-me page starters
    public/ auth/ youth/ mentor/ admin/ guardian/ volunteer/ shared/ states/
  contract.md               # the component/class + JS API spec every page follows
  backend-implementation-plan.md
```

All sample data (youth, mentors, cohorts, lessons) is **fictional** and kept consistent across pages for the demo.
