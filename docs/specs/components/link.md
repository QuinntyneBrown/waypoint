# Link

| Field | Value |
|---|---|
| Selector | `a[w-link]` |
| Library path | `frontend/projects/components/src/lib/link/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010 |
| Design system | [`link.html`](../../design-system/components/link.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) that links to a page, file or site |
| Rendering | [`link.html`](link.html) |

## Purpose and scope

A link navigates. In Waypoint, links are ink (lime in dark) and underlined inside running text, because colour alone does not separate them from ink body copy. If activating it changes data, use the [Button](button.md); if it must look like an action but navigates, use `a[w-button]`.

Out of scope:

- Navigation items in the sidebar, bottom bar and tabs (see Shell and Tabs).
- Raw URLs as link text; pages write a destination label.

## Usage

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Top of detail pages | `standalone` | "Back to cohorts", "Back to reviews" | default, hover | canvas |
| Submission review and status | inline, `download` | "reflection-week-3.pdf", an external link title | default, hover | card |
| Running text | inline | "help centre" | default, hover, focus | canvas, card |

## Anatomy

1. **Label** — describes the destination.
2. **Underline** — 1px at rest in inline links, 2px on hover, 3px offset.
3. **Icon** (optional) — `w-icon`, 1em, `aria-hidden`.

Host: the native `a` element; the component projects its content unchanged.

## API

### Inputs

| Input | Type | Default | Required | Rule |
|---|---|---|---|---|
| `standalone` | `boolean` | `false` | no | Link on its own line or beside an icon: no underline at rest, underline on hover. |

### Outputs

None. A native `click` event is the interface.

### Content slots

| Slot | Accepts | Rule |
|---|---|---|
| default | text, optional `w-icon`, optional `w-sr-only` text | The label; an external link adds visually hidden "(opens in a new tab)". |

## Variants and sizes

| Variant | Class | Use for |
|---|---|---|
| Inline | none | Links inside running text. |
| Standalone | `w-link--standalone` | Back links and links with an icon on their own line. |

Links inherit the size of the text around them; there are no size variants.

## States

| State | Trigger | Visual | Assistive technology |
|---|---|---|---|
| Default | none | `--color-fg-link`, underlined (inline). | Role link, name from content. |
| Hover | `:hover` | `--color-fg-link-hover`, 2px underline. | none |
| Focus | `:focus-visible` | Focus ring. | Focus announced with name and role. |
| Visited | `:visited` | Same colour as default. | none |

## Markup

```html
<!-- rendered: inline -->
<a w-link class="w-link" href="/help">help centre</a>
```

```html
<!-- consumer -->
<a w-link standalone routerLink="/admin/cohorts">Back to cohorts</a>
<a w-link [href]="url" [download]="file.fileName">{{ file.fileName }}</a>
```

## Design

- The `w-link` and `w-link--standalone` classes stay on the host.
- Weight medium; underline offset 3px; thickness 1px at rest and 2px on hover; focus ring from the global focus tokens.
- Visited links keep the same colour: lists are task lists, and a colour change would read as done.
- Link rules are new to the library (they were only documented in the design system); the component adds them.

Component tokens:

| Token | Aliases | Overridden by |
|---|---|---|
| None | none | none |

## Colour

| Part | Token | Light | Dark |
|---|---|---|---|
| Link | `--color-fg-link` | ink | lime-300 |
| Link hover | `--color-fg-link-hover` | black | lime-100 |

| Foreground | Background | Minimum | Use |
|---|---|---|---|
| `--color-fg-link` | `--color-bg-surface` | 4.5:1 | Link text on a card |
| `--color-fg-link` | `--color-bg-canvas` | 4.5:1 | Link text on the page |

## Responsive behaviour

- Long labels and file names wrap with `overflow-wrap: anywhere`; they never force horizontal scroll.
- Standalone links in cards keep a 24px minimum target height.
- At 320px nothing scrolls horizontally or clips.

## Accessibility

### Role and pattern

Native `a` with an `href` (or `routerLink`). No ARIA role.

### Keyboard

| Key | Action |
|---|---|
| <kbd>Tab</kbd> | Moves focus to the link in DOM order. |
| <kbd>Enter</kbd> | Follows the link. |

### Focus

Visible `:focus-visible` ring in both themes.

### Labelling

The visible label names the destination. An external link opened in a new tab includes visually hidden "(opens in a new tab)".

### Announcements

None beyond role and name.

### Motion

None.

## Content and internationalisation

- Describe the destination: "Back to cohorts", not "Click here". Never show a raw URL as the label.
- Translatable inputs: none (content is projected). Data values: file names.

## Performance

- Change detection: `OnPush`; one boolean input, host bindings only.
- No dedicated perf-test scenario exists in this repository.
- Layout stability: no size change between states.

## Acceptance criteria

### Rendering

- **AC-1** Given `<a w-link href="/help">help centre</a>` inside a paragraph, when it renders, then it carries the class `w-link`, is underlined, and keeps its text. (L2-NFR-010)
- **AC-2** Given `standalone`, when it renders, then it carries `w-link--standalone`, is not underlined at rest, and is underlined on hover. (L2-NFR-010)

### Keyboard and focus

- **AC-3** Given a "Back to cohorts" link, when a user tabs to it and presses Enter, then the focus ring is visible and the cohorts page opens. (L2-NFR-010)

### Screen readers

- **AC-4** Given an external link that opens a new tab, when read by a screen reader, then the name ends with "(opens in a new tab)". (L2-NFR-010)

### Theming

- **AC-5** Given the dark theme, when an inline link renders on the page, then its colour contrast is at least 4.5:1 and it differs from body text by more than colour (underline). (L2-NFR-010)

### Responsive

- **AC-6** Given a 320px viewport and a file name of 40 characters, when the link renders in a card, then it wraps and the page does not scroll horizontally. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/link/`: `w-link.ts`, spec, story. Selector `a[w-link]`, input `standalone`; `ViewEncapsulation.None`.
- Replaces `class="w-link"` in 9 call sites. Back links become `standalone`; file links stay inline. One `<span class="w-link">` (a styled label inside a card link on the notifications page) loses the class, because `.w-link` had no CSS and a span is not a link.
- `.w-link` had no CSS in `core.css` before; the new rules are the only visual change (inline links gain an underline).

## Decisions

- **D-1** *Back links: inline or standalone?* Standalone. They sit on their own line and today show no underline until hover, which matches the standalone variant.
- **D-2** *The `span.w-link`?* Plain text. A span is not a link and the class styled nothing.
