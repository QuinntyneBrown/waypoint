# Waypoint — HTML mocks

Static, high-fidelity design mocks for **Waypoint**, a youth leadership platform supporting Bible study, mentoring, leadership development, coding, program operations, safeguarding, and progress tracking.

Open [`index.html`](index.html) in a browser. It is the searchable product map for the complete mock set and links to dialog, empty, loading, and other state variants. No server, package install, build step, or internet connection is required.

These files are an interaction and visual reference, not the production application. All people, cohorts, lessons, messages, progress, and other sample information are fictional.

## Subsystems and screens

Screen mocks use the same top-level subsystem structure as [`../specs`](../specs/README.md). Within each subsystem, a role folder preserves the screen's audience and avoids filename collisions such as the youth, mentor, and admin calendar views.

| Subsystem                       | Screens | Role folders                            | Coverage                                                        |
| ------------------------------- | ------: | --------------------------------------- | --------------------------------------------------------------- |
| `application-intake/`           |       3 | `admin/`, `public/`                     | Application, confirmation, and review                            |
| `authentication/`               |      11 | `admin/`, `auth/`                       | Sign-in, invitations, recovery, shared-device, roles, and setup  |
| `consent-and-safeguarding/`     |       2 | `admin/`, `auth/`                       | Initial guardian consent and the safeguarding audit log          |
| `curriculum-authoring/`         |       7 | `admin/`                                | Tracks, modules, lessons, quizzes, assignments, badges, and progression |
| `engagement/`                   |      12 | `admin/`, `mentor/`, `youth/`           | Calendars, attendance, announcements, messages, and notifications |
| `guardian-portal/`              |       4 | `guardian/`                             | Guardian home, progress, news, and consent                       |
| `learning-experience/`          |      17 | `youth/`                                | Curriculum, lessons, quizzes, reflections, badges, and submissions |
| `mentoring/`                    |      13 | `mentor/`                               | Dashboard, roster, grading, competencies, check-ins, and reviews |
| `non-functional/`               |       0 | —                                       | Cross-cutting quality is demonstrated throughout the mock set    |
| `platform-services/`            |      20 | `admin/`, `mentor/`, `shared/`, `states/`, `youth/` | Profiles, settings, help, support, and system states   |
| `program-administration/`       |       6 | `admin/`                                | People, cohorts, progress board, and organization settings       |
| `public-site/`                  |       6 | `public/`                               | Landing, involvement, contact, about, programs, and privacy      |
| `reporting-and-data/`           |       7 | `admin/`                                | Dashboard, reports, exports, and data import                     |
| `volunteer-portal/`             |       3 | `volunteer/`                            | Volunteer home, schedule, and attendance                         |

The root also contains the product map and reusable page templates, bringing the complete reference to 118 HTML files.

## Design system

The mocks are styled by the Waypoint design system in [`../design-system/`](../design-system/README.md); open [`../design-system/index.html`](../design-system/index.html) for tokens, components and patterns in light and dark. Every page links:

- `../design-system/assets/fonts.css` — embedded font resources
- `../design-system/tokens/tokens.css` — design tokens (colour, type, spacing, layout, shape, elevation, motion), light and dark
- `../design-system/assets/components.css` — the `w-*` component, shell and utility classes
- `assets/js/waypoint.js` — shared shell, icons, dialogs, toasts, state helpers, and fictional data (the only mock-local asset)

Page-local styles reference design-system tokens (`var(--color-*)`, `var(--space-*)` …), never raw values.

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
5. Compare shared components against the [design system](../design-system/index.html) and the rules in `../contract.md`.

Implementation work should also trace behaviour to the owning requirements in [`../specs`](../specs/README.md). The mocks communicate intended experience; the requirements and server API remain authoritative for validation, authorization, consent, and safeguarding rules.
