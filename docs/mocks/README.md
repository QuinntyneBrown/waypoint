# Word Up — HTML mocks

Static, high-fidelity design mocks for **Word Up**, a youth leadership platform supporting Bible study, mentoring, leadership development, coding, program operations, safeguarding, and progress tracking.

Open [`index.html`](index.html) in a browser. It is the searchable product map for the complete mock set and links to dialog, empty, loading, and other state variants. No server, package install, build step, or internet connection is required.

These files are an interaction and visual reference, not the production application. All people, cohorts, lessons, messages, progress, and other sample information are fictional.

## Product roles and screens

| Folder                       | Screens | Coverage                                                                                 |
| ---------------------------- | ------: | ---------------------------------------------------------------------------------------- |
| `public/`                    |       8 | Landing, application, involvement, contact, about, programs, and privacy                 |
| `auth/`                      |      12 | Sign-in, PIN, invitations, consent, recovery, shared-device, and setup flows             |
| `youth/`                     |      23 | Home, journey, Bible and coding lessons, quizzes, reflections, progress, and submissions |
| `mentor/`                    |      19 | Dashboard, roster, assignments, grading, competency, check-ins, attendance, and reviews  |
| `admin/`                     |      29 | Cohorts, curriculum, people, attendance, applications, reports, audit, and settings      |
| `guardian/` and `volunteer/` |       7 | Guardian progress and consent; volunteer schedule and attendance                         |
| `shared/` and `states/`      |      17 | Profiles, settings, help, support, search, offline, permission, and error states         |

The folder also contains the product map, style guide, and reusable page templates, bringing the complete reference to more than 120 HTML files.

## Design system

[`styleguide.html`](styleguide.html) is the rendered design-system reference. Shared tokens, fonts, layout primitives, navigation, forms, tables, cards, feedback, and responsive behaviour live in:

- `assets/css/_core.css` — tokens and component foundations
- `assets/css/_fonts.css` — embedded font resources
- `assets/css/wordup.css` — bundled styles consumed by every page
- `assets/js/wordup.js` — shared shell, icons, dialogs, toasts, state helpers, and fictional data

The mocks use FaithTech's visual language, including the signature lime brand colour, Space Grotesk headings, Inter body text, and Fraunces accents. Fonts are embedded, icons are inline SVG, and avatars are generated from initials so the screens work offline.

## Shared page contract

Read [`../contract.md`](../contract.md) before changing or adding a mock. It defines the allowed CSS classes, shared JavaScript API, navigation shells, asset paths, accessibility patterns, and filename conventions.

Reusable starters live at the root of this folder:

- `_template-public.html`
- `_template-auth.html`
- `_template-mobile.html`
- `_template-desktop.html`
- `_template-state.html`

Copy the closest template and an existing screen with the same interaction pattern. Do not introduce page-local design tokens or external dependencies.

## Reviewing the mocks

1. Open `index.html` from the filesystem.
2. Filter by role or search for a screen.
3. Use the linked `?open=…` and `?state=…` variants to review dialogs and non-default states.
4. Check both narrow and wide viewport layouts.
5. Compare shared components against `styleguide.html` and the rules in `../contract.md`.

Implementation work should also trace behaviour to the owning requirements in [`../specs`](../specs/README.md). The mocks communicate intended experience; the requirements and server API remain authoritative for validation, authorization, consent, and safeguarding rules.
