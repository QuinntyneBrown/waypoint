# Waypoint design system

Extracted from [docs/mocks](../mocks/README.md) on 2026-10-15. Open [index.html](index.html) locally.

The system keeps the mocks' `w-*` class vocabulary, so mocks and product share one language, and replaces the mocks' `--w-*` draft tokens with a three-tier token set that passes WCAG 2.2 AA in light and dark.

**Status (v1):** this system is the design reference for the mocks. The Angular frontend still styles itself with its own `--w-*` tokens in `frontend/projects/components/src/styles`; migrating it to these tokens is not planned for v1 (DECISIONS #21).

## Foundations

| Page | Covers |
|---|---|
| [Color](foundations/color.html) | Ramps, roles, both themes, live contrast pairs |
| [Typography](foundations/typography.html) | Families, scale, roles, rhythm |
| [Spacing](foundations/spacing.html) | 4px scale and usage |
| [Layout](foundations/layout.html) | Breakpoints, grid, containers, templates |
| [Elevation](foundations/elevation.html) | Shadows, surfaces, z-order |
| [Shape](foundations/shape.html) | Radii and border widths |
| [Motion](foundations/motion.html) | Durations, easings, reduced motion |
| [Iconography](foundations/iconography.html) | Icon set, sizes, labelling |
| [Theming](foundations/theming.html) | Tiers, dark, adding a theme, tokens.json |
| [Responsive](foundations/responsive.html) | Per-component breakpoint behaviour, touch, testing |
| [Accessibility](foundations/accessibility.html) | WCAG 2.2 AA commitments and testing |
| [Content](foundations/content.html) | Voice, mechanics, formulas, microcopy |

## Components

| Component | Group | Variants | States | Source mocks |
|---|---|---|---|---|
| [Button](components/button.html) | Actions | yes | Default, Hover, Focus, Active, Disabled, Loading | [applications.html](../mocks/application-intake/admin/applications.html), [apply.html](../mocks/application-intake/public/apply.html), [lesson-editor.html](../mocks/curriculum-authoring/admin/lesson-editor.html) |
| [Icon button](components/icon-button.html) | Actions | yes | Default, Hover, Focus, Active, Disabled | [_template-desktop.html](../mocks/_template-desktop.html), [applications.html](../mocks/application-intake/admin/applications.html), [users-roles.html](../mocks/authentication/admin/users-roles.html) |
| [Button group](components/button-group.html) | Actions | yes | Default, Hover, Focus, Pressed, Disabled | [applications.html](../mocks/application-intake/admin/applications.html), [welcome-setup.html](../mocks/authentication/auth/welcome-setup.html), [lesson-editor.html](../mocks/curriculum-authoring/admin/lesson-editor.html) |
| [Link](components/link.html) | Actions | yes | Default, Hover, Focus, Visited | [help.html](../mocks/platform-services/shared/help.html), [about.html](../mocks/platform-services/shared/about.html), [about.html](../mocks/public-site/public/about.html) |
| [Menu](components/menu.html) | Actions | yes | Default, Hover, Focus, Disabled, Checked | [waypoint.js](../mocks/assets/js/waypoint.js), [_template-desktop.html](../mocks/_template-desktop.html) |
| [Text field](components/text-field.html) | Inputs | yes | Default, Hover, Focus, Invalid, Disabled, Read-only | [apply.html](../mocks/application-intake/public/apply.html), [login.html](../mocks/authentication/auth/login.html), [reset-password.html](../mocks/authentication/auth/reset-password.html) |
| [Textarea](components/textarea.html) | Inputs | yes | Default, Hover, Focus, Invalid, Disabled, Read-only | [apply.html](../mocks/application-intake/public/apply.html), [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html) |
| [Select](components/select.html) | Inputs | yes | Default, Hover, Focus, Invalid, Disabled | [applications.html](../mocks/application-intake/admin/applications.html), [apply.html](../mocks/application-intake/public/apply.html), [invitations.html](../mocks/authentication/admin/invitations.html) |
| [Checkbox](components/checkbox.html) | Inputs | yes | Default, Hover, Focus, Checked, Indeterminate, Invalid, Disabled | [apply.html](../mocks/application-intake/public/apply.html), [login.html](../mocks/authentication/auth/login.html), [settings-notifications.html](../mocks/platform-services/shared/settings-notifications.html) |
| [Radio group](components/radio-group.html) | Inputs | yes | Default, Hover, Focus, Checked, Invalid, Disabled | [apply.html](../mocks/application-intake/public/apply.html), [quiz-builder.html](../mocks/curriculum-authoring/admin/quiz-builder.html), [badge-builder.html](../mocks/curriculum-authoring/admin/badge-builder.html) |
| [Switch](components/switch.html) | Inputs | yes | Off, Hover, Focus, On, Disabled | [users-roles.html](../mocks/authentication/admin/users-roles.html), [welcome-setup.html](../mocks/authentication/auth/welcome-setup.html), [settings-notifications.html](../mocks/platform-services/shared/settings-notifications.html) |
| [File upload](components/file-upload.html) | Inputs | yes | Default, Dragging, Failed | [submit-work.html](../mocks/learning-experience/youth/submit-work.html), [support.html](../mocks/platform-services/shared/support.html) |
| [Form field](components/form-field.html) | Inputs | yes | Default, Invalid | [apply.html](../mocks/application-intake/public/apply.html), [invitations.html](../mocks/authentication/admin/invitations.html), [reset-password.html](../mocks/authentication/auth/reset-password.html) |
| [Form layout](components/form-layout.html) | Inputs | yes | Default, Submitting | [apply.html](../mocks/application-intake/public/apply.html), [invitations.html](../mocks/authentication/admin/invitations.html), [welcome-setup.html](../mocks/authentication/auth/welcome-setup.html) |
| [Top bar](components/top-bar.html) | Navigation | yes | Default, Hover, Focus | [_template-desktop.html](../mocks/_template-desktop.html), [_template-mobile.html](../mocks/_template-mobile.html), [_template-public.html](../mocks/_template-public.html) |
| [Sidebar navigation](components/sidebar-navigation.html) | Navigation | yes | Default, Hover, Focus, Current | [_template-desktop.html](../mocks/_template-desktop.html), [waypoint.js](../mocks/assets/js/waypoint.js) |
| [Bottom navigation](components/bottom-navigation.html) | Navigation | yes | Default, Hover, Focus, Current | [_template-mobile.html](../mocks/_template-mobile.html), [waypoint.js](../mocks/assets/js/waypoint.js) |
| [Tabs](components/tabs.html) | Navigation | yes | Default, Hover, Focus, Selected, Disabled | [invitations.html](../mocks/authentication/admin/invitations.html), [progress.html](../mocks/learning-experience/youth/progress.html), [reviews.html](../mocks/mentoring/mentor/reviews.html) |
| [Breadcrumb](components/breadcrumb.html) | Navigation | yes | Default, Hover, Focus, Current | [lesson-editor.html](../mocks/curriculum-authoring/admin/lesson-editor.html), [module-editor.html](../mocks/curriculum-authoring/admin/module-editor.html) |
| [Pagination](components/pagination.html) | Navigation | yes | Default, Hover, Focus, Current, Disabled | [applications.html](../mocks/application-intake/admin/applications.html), [audit-log.html](../mocks/consent-and-safeguarding/admin/audit-log.html), [users-roles.html](../mocks/authentication/admin/users-roles.html) |
| [Stepper](components/stepper.html) | Navigation | yes | Upcoming, Current, Complete, Locked, Error | [lesson-editor.html](../mocks/curriculum-authoring/admin/lesson-editor.html), [module-editor.html](../mocks/curriculum-authoring/admin/module-editor.html), [tracks.html](../mocks/curriculum-authoring/admin/tracks.html) |
| [Footer](components/footer.html) | Navigation | yes | static | [about.html](../mocks/public-site/public/about.html), [apply.html](../mocks/application-intake/public/apply.html), [application-submitted.html](../mocks/application-intake/public/application-submitted.html) |
| [Skip link](components/skip-link.html) | Navigation | single | Hidden, Focused | [_template-desktop.html](../mocks/_template-desktop.html) |
| [Card](components/card.html) | Containers | yes | Default, Hover, Focus | [applications.html](../mocks/application-intake/admin/applications.html), [home.html](../mocks/guardian-portal/guardian/home.html), [progress.html](../mocks/learning-experience/youth/progress.html) |
| [Accordion](components/accordion.html) | Containers | yes | Default, Hover, Focus, Expanded | [help.html](../mocks/platform-services/shared/help.html) |
| [Dialog](components/dialog.html) | Containers | yes | Default, Busy | [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html), [users-roles.html](../mocks/authentication/admin/users-roles.html) |
| [Drawer](components/drawer.html) | Containers | yes | Default, Busy | [_template-mobile.html](../mocks/_template-mobile.html), [waypoint.js](../mocks/assets/js/waypoint.js), [applications.html](../mocks/application-intake/admin/applications.html) |
| [Tooltip](components/tooltip.html) | Containers | yes | Hidden, Visible | [applications.html](../mocks/application-intake/admin/applications.html), [users-roles.html](../mocks/authentication/admin/users-roles.html) |
| [Divider](components/divider.html) | Containers | yes | static | [apply.html](../mocks/application-intake/public/apply.html), [users-roles.html](../mocks/authentication/admin/users-roles.html), [login.html](../mocks/authentication/auth/login.html) |
| [Container, grid and stack](components/layout.html) | Containers | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [home.html](../mocks/guardian-portal/guardian/home.html), [_template-desktop.html](../mocks/_template-desktop.html) |
| [Table](components/table.html) | Data display | yes | Default, Hover, Selected | [applications.html](../mocks/application-intake/admin/applications.html), [users-roles.html](../mocks/authentication/admin/users-roles.html), [invitations.html](../mocks/authentication/admin/invitations.html) |
| [List](components/list.html) | Data display | yes | Default, Hover, Focus, Selected | [notifications.html](../mocks/engagement/mentor/notifications.html), [notifications.html](../mocks/engagement/youth/notifications.html), [messages.html](../mocks/engagement/mentor/messages.html) |
| [Message thread](components/message-thread.html) | Data display | single | static | [messages.html](../mocks/engagement/youth/messages.html), [messages.html](../mocks/engagement/mentor/messages.html) |
| [Description list](components/description-list.html) | Data display | yes | static | [about.html](../mocks/platform-services/shared/about.html), [applications.html](../mocks/application-intake/admin/applications.html) |
| [Avatar](components/avatar.html) | Data display | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [users-roles.html](../mocks/authentication/admin/users-roles.html), [messages.html](../mocks/engagement/mentor/messages.html) |
| [Badge](components/badge.html) | Data display | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html), [audit-log.html](../mocks/consent-and-safeguarding/admin/audit-log.html) |
| [Chip](components/chip.html) | Data display | yes | Default, Hover, Focus, Selected, Disabled | [calendar.html](../mocks/engagement/admin/calendar.html), [tracks.html](../mocks/curriculum-authoring/admin/tracks.html), [notifications.html](../mocks/engagement/mentor/notifications.html) |
| [Stat card](components/stat.html) | Data display | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html), [badge-builder.html](../mocks/curriculum-authoring/admin/badge-builder.html) |
| [Timeline](components/timeline.html) | Data display | single | static | [application-submitted.html](../mocks/application-intake/public/application-submitted.html), [progression-editor.html](../mocks/curriculum-authoring/admin/progression-editor.html), [child-progress.html](../mocks/guardian-portal/guardian/child-progress.html) |
| [Toast](components/toast.html) | Feedback | yes | Visible, Leaving | [waypoint.js](../mocks/assets/js/waypoint.js), [_template-state.html](../mocks/_template-state.html) |
| [Alert](components/alert.html) | Feedback | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [application-submitted.html](../mocks/application-intake/public/application-submitted.html), [apply.html](../mocks/application-intake/public/apply.html) |
| [Inline message](components/inline-message.html) | Feedback | yes | static | [apply.html](../mocks/application-intake/public/apply.html), [reset-password.html](../mocks/authentication/auth/reset-password.html) |
| [Progress](components/progress.html) | Feedback | yes | static | [progress.html](../mocks/learning-experience/youth/progress.html), [child-progress.html](../mocks/guardian-portal/guardian/child-progress.html), [home.html](../mocks/guardian-portal/guardian/home.html) |
| [Spinner](components/spinner.html) | Feedback | yes | static | [_template-state.html](../mocks/_template-state.html) |
| [Skeleton](components/skeleton.html) | Feedback | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html), [users-roles.html](../mocks/authentication/admin/users-roles.html) |
| [Empty state](components/empty-state.html) | Feedback | yes | static | [applications.html](../mocks/application-intake/admin/applications.html), [invitations.html](../mocks/authentication/admin/invitations.html), [application-submitted.html](../mocks/application-intake/public/application-submitted.html) |
| [Error page](components/error-page.html) | Feedback | yes | static | [account-locked.html](../mocks/authentication/auth/account-locked.html), [_template-state.html](../mocks/_template-state.html) |
| [Scripture quote](components/scripture.html) | Domain | single | static | [lesson-editor.html](../mocks/curriculum-authoring/admin/lesson-editor.html), [module-editor.html](../mocks/curriculum-authoring/admin/module-editor.html), [home.html](../mocks/guardian-portal/guardian/home.html) |
| [Quiz option](components/quiz-option.html) | Domain | yes | Default, Hover, Focus, Selected | [quiz.html](../mocks/learning-experience/youth/quiz.html), [quiz-result.html](../mocks/learning-experience/youth/quiz-result.html), [quiz-builder.html](../mocks/curriculum-authoring/admin/quiz-builder.html) |
| [Achievement badge](components/achievement-badge.html) | Domain | yes | static | [badges.html](../mocks/learning-experience/youth/badges.html), [badge-builder.html](../mocks/curriculum-authoring/admin/badge-builder.html), [child-progress.html](../mocks/guardian-portal/guardian/child-progress.html) |

## Patterns

| Pattern | Covers |
|---|---|
| [Forms](patterns/forms.html) | Forms |
| [Feedback and loading](patterns/feedback.html) | Feedback and loading |
| [Empty and error states](patterns/empty-and-error.html) | Empty and error states |
| [Navigation and page structure](patterns/navigation.html) | Navigation and page structure |
| [Dialogs and overlays](patterns/dialogs-and-overlays.html) | Dialogs and overlays |
| [Data tables and lists](patterns/tables-and-lists.html) | Data tables and lists |
| [Notifications](patterns/notifications.html) | Notifications |
| [Content and tone](patterns/content-and-tone.html) | Content and tone |

## Tokens

- `tokens/tokens.css` — source of truth (primitives, semantic, light + dark, preference overrides).
- `tokens/tokens.json` — DTCG export generated from tokens.css.
- `tokens/contrast-pairs.json` — every fg/bg pair the components rely on; checked in both themes.
- `assets/components.css` — the component stylesheet (token-only).
- `assets/fonts.css` — the embedded Inter, Space Grotesk and Fraunces faces, copied from the mocks.

## Drift found in the mocks

| Mock | Issue | Resolution |
|---|---|---|
| assets/css/_core.css (inputs, check, radio) | `--w-border-strong` (28% ink) was 1.9:1 on white; fails 1.4.11. | `--color-border-strong` = ink-500, 3.6:1. |
| assets/css/_core.css (placeholders, empty icon) | `--w-ink-45` text 2.6:1. | `--color-fg-subtle` / `--color-fg-muted` (5.9:1). |
| assets/css/_core.css (muted text) | `--w-muted` 60% ink alpha: 4.7:1 on white but 4.2:1 on greige. | Opaque `--color-fg-muted` (ink-600), ≥ 5.4:1 on all light surfaces. |
| assets/css/_core.css (input focus) | 3px lime halo (`--w-lime-soft`) was 1.2:1; fails 2.4.11. | Solid 2px ring in `--color-focus-ring`. |
| assets/css/_core.css (checkbox, progress) | Lime fills on white/greige 1.1–1.2:1. | Checked boxes and progress fills use `--color-accent` (ink/lime). |
| assets/css/_core.css (switch) | Off track #cdcdcd 1.6:1. | `--color-border-strong`. |
| assets/css/_core.css (danger button, error text) | White on #d5421b 4.2:1; error text 4.2:1. | Danger solid tomato-600; error text tomato-700 (6.3:1). |
| assets/css/_core.css (badges) | Hard-coded tone text colours on alpha tints. | Status `-fg`/`-bg` tokens with opaque tints. |
| assets/css/_core.css (offline syncing banner) | White on teal #1d8fb9 3.6:1. | `--color-info-solid` (teal-500, 5.3:1). |
| assets/css/_core.css (type) | 43 font sizes, ten below 12px (.64–.72rem). | Nine-step scale, minimum 12px. |
| assets/css/_core.css (radii) | 20 radii (5, 7, 10px…). | Five radii + full. |
| assets/css/_core.css (spacing) | Off-grid 6, 9, 13, 14, 22px paddings. | Snapped to the 4px scale. |
| assets/css/_core.css (sizes) | Icon button 38px, sidebar 244px, avatar xs 26px. | 44px, 240px, 24px. |
| assets/css/_core.css (breakpoints) | 560/600/620/760/820/900/960px. | 640/768/960/1280/1440px. |
| assets/css/_core.css (motion) | Nine durations .12–.4s. | Four duration tokens. |
| assets/css/_core.css (links) | Links not underlined (`color:inherit`); fails 1.4.1. | `.w-link` underlined at rest. |
| assets/js/waypoint.js (menus) | Menu items are divs with click handlers; inline positioning. | APG menu button with buttons. |
| assets/js/waypoint.js (shell) | Burger lacks `aria-expanded`; no skip link. | Added both. |
| Several pages (tabs, segmented, chips) | Selection by `.is-active` class only. | Styles keyed to `aria-selected`/`aria-pressed`/`aria-current`. |
| All progress bars (104 uses) | No progressbar semantics. | `role="progressbar"` with values. |
| help, about, messages, notifications, support, submit-work | Page-local components (faq, meta-row, msg-bubble, notif, up-drop, sw-drop). | Promoted to accordion, description list, message thread, list, file upload. |
| All mocks | No dark theme. | Dark theme built from roles. |

## Changelog

- 2026-10-15: initial extraction.

## Verification

Run on 2026-10-15 with the extracting-design-systems scripts:

- `check_contrast.py tokens/tokens.css`: 139 pairs passed, 0 failed (light and dark).
- `check_design_system.py docs/design-system`: 71 pages (12 foundations, 50 components, 8 patterns), 0 errors, 0 warnings.
- `tokens_to_json.py`: 268 tokens exported to `tokens/tokens.json`.
- Screenshots: index, button, table, dialog, text field, checkbox and color pages at 360px and 1280px in both themes. Every page was checked for horizontal overflow at 360px: none.
- Loop back to the mocks: all 118 mocks were rendered in Chromium with their stylesheets swapped for `assets/fonts.css` + `tokens/tokens.css` + `assets/components.css` (injected at runtime, mock files unchanged). Result at 360px and 1280px: no horizontal overflow, no script errors. Remaining visual differences are the corrections in the drift table above (darker borders and muted text, solid focus rings, ink checkboxes and progress fills, 44px icon buttons, 12px minimum type).
- Mocks re-pointed: all 118 mocks now link `../design-system/assets/fonts.css`, `tokens/tokens.css` and `assets/components.css` instead of `assets/css/waypoint.css`; page-local `var(--w-*)` references in 54 mocks were rewritten to system tokens. Re-check: no horizontal overflow at 360/1280px, no script errors, tokens resolve on every mock (the `_template-*` files keep their two-level-deep relative paths by design).
