# Button

| Field | Value |
|---|---|
| Selector | `button[w-button]`, `a[w-button]` |
| Library path | `frontend/projects/components/src/lib/button/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010, L2-PLATFORM-004 |
| Design system | [`button.html`](../../design-system/components/button.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) with Button (109 of 112) |
| Rendering | [`button.html`](button.html) |

## Purpose and scope

A button starts an action: save, send, invite, delete. One primary button per view tells people what matters most. Use a link (`a` without `w-button`, or the Link component) when activating it only navigates; use the [Icon button](icon-button.md) for a standalone icon control in a toolbar or card header. A `w-button` on an `a` element is for navigation that must look like an action ("Take attendance", "Back to my journey").

Out of scope:

- Which button is primary on a page, and the confirmation dialog around a danger action. The page decides.
- Button groups and segmented toggles (see Button group).
- Menus opened by a button (see Menu).

## Usage

Counts come from the mock inventory of `w-btn` (all subsystems) and the application templates.

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Page header, form footer, card footer | `primary`, `md` | "Save changes", "New session", "Send request" | default, busy, disabled | canvas, card, dialog |
| Cancel, Export, Edit in a card header | `ghost`, `md` | "Cancel", "Export", "Close" | default, disabled | canvas, card, dialog |
| Table rows, toolbars, list rows | `ghost`/`primary`/`secondary`, `sm` | "View", "Assign", "Resend" | default, disabled | table, card |
| Back links above a page | `ghost`, `sm`, `a` | "Back to my youth" | default | canvas |
| Dense toolbars, filter rows | `subtle`, `sm` | "Add criterion", "All present" | default | card, canvas |
| Strong non-primary action | `secondary`, `md`/`sm` | "Message", "Edit", "Review" | default | card, canvas |
| Destructive confirm | `danger`, `md`/`sm` | "Remove person", "Revoke", "Delete badge" | default, busy | dialog, row |
| Auth and public forms | `primary`, `lg`, `block` | "Sign in", "Submit application", "Give consent" | default, busy | auth card |
| Narrow stacks | any variant, `block` | "Log a check-in", "Give $20" | default | card, drawer |
| Low-emphasis action in text | `link` | "Learn more" | default | canvas |
| Leading or trailing icon | any variant | `w-icon` + label | default | all |
| Icon-only (back, exit) | `ghost`, `iconOnly` | `w-icon`, `aria-label` | default | quiz and lesson top bars |

## Anatomy

1. **Leading icon** (optional) — `w-icon`, 18px, inherits the label colour, `aria-hidden`.
2. **Label** — Space Grotesk 500, a verb phrase in sentence case; the accessible name.
3. **Trailing icon** (optional) — same rules as the leading icon.
4. **Container** — pill shape, minimum height 44px, 1.5px border slot.

Host: the native `button` or `a` element itself. The component adds classes and attributes to it; it renders no wrapper and projects all content unchanged.

## API

### Inputs

| Input | Type | Default | Required | Rule |
|---|---|---|---|---|
| `variant` | `'primary' \| 'secondary' \| 'ghost' \| 'subtle' \| 'neutral' \| 'danger' \| 'link'` | `'neutral'` | no | Emphasis. Danger is for confirmations only. |
| `size` | `'sm' \| 'md' \| 'lg'` | `'md'` | no | Small in tables and toolbars, large on auth and public heroes. |
| `block` | `boolean` | `false` | no | Fills the container width. |
| `iconOnly` | `boolean` | `false` | no | Square padding. The consumer must set `aria-label`. |
| `busy` | `boolean` | `false` | no | Shows a spinner, keeps width and focus, ignores activation. |

Native attributes (`type`, `disabled`, `href`, `routerLink`, `aria-*`, `(click)`) stay on the element and are the consumer's.

### Outputs

None. A native `click` event is the interface; the component adds no output.

### Content slots

| Slot | Accepts | Rule |
|---|---|---|
| default | text, `w-icon` | The label, with optional icons on either side. Icon-only buttons contain only a `w-icon`. |

## Variants and sizes

| Variant | Class | Use for |
|---|---|---|
| Primary | `w-btn--primary` | The one action the view exists for. Never two per view. |
| Secondary | `w-btn--secondary` | A strong action that is not the main one. |
| Ghost | `w-btn--ghost` | Everyday: Cancel, Export, Edit. |
| Subtle | `w-btn--subtle` | Dense surfaces: filters, row tools. |
| Neutral | none | Default fill, lowest decision weight. |
| Danger | `w-btn--danger` | Inside a confirmation. |
| Link | `w-btn--link` | Low-emphasis action inside text. |

| Size | Class | Height | Padding | Type |
|---|---|---|---|---|
| sm | `w-btn--sm` | 44px minimum | 8px 14px | 0.85rem |
| md | none | 44px | 13px 22px | 0.95rem |
| lg | `w-btn--lg` | 52px | 16px 28px | 1.05rem |

## States

| State | Trigger | Visual | Assistive technology |
|---|---|---|---|
| Default | none | Variant fill and label colour. | Role button (or link), name from content. |
| Hover | `:hover` | Variant hover fill. | none |
| Focus | `:focus-visible` | Global focus ring. | Focus announced with name and role. |
| Active | `:active` | Pressed fill. | none |
| Disabled | native `disabled` | 50% opacity, not focusable, not-allowed cursor. | Announced as dimmed or unavailable. |
| Busy | `busy` input | Label hidden behind a spinner, width unchanged. | `aria-busy="true"`; focus kept; activation ignored. |

## Markup

```html
<!-- rendered: primary, md -->
<button w-button variant="primary" type="button" class="w-btn w-btn--primary">Save changes</button>
```

```html
<!-- rendered: busy -->
<button w-button variant="primary" type="submit" class="w-btn w-btn--primary is-loading" aria-busy="true">Save changes</button>
```

```html
<!-- consumer -->
<button w-button variant="primary" type="submit" [busy]="saving()">Save changes</button>
<a w-button variant="ghost" size="sm" routerLink="/mentor/youth">Back to my youth</a>
<button w-button variant="ghost" iconOnly aria-label="Exit quiz" type="button"><w-icon name="x" /></button>
```

## Design

- The class names `w-btn`, `w-btn--<variant>`, `w-btn--<size>`, `w-btn--block`, `w-btn--icon` and `is-loading` are applied to the host so existing selectors in the library and in page stylesheets keep working.
- Shape `--radius-full`; border `--border-width-accent`; type Space Grotesk via the heading font token; transitions on background, colour and border colour only.
- The component owns the button rules; they move out of `core.css`. The lime-card override (`.w-card--lime .w-btn--primary`) stays in the Card styles.

Component tokens:

| Token | Aliases | Overridden by |
|---|---|---|
| `--btn-bg` | `--color-bg-subtle-hover` | variant |
| `--btn-fg` | `--color-fg-default` | variant |

## Colour

| Part | Token | Light | Dark |
|---|---|---|---|
| Primary fill | `--color-brand` | brand | brand |
| Primary label | `--color-fg-on-brand` | ink | ink |
| Secondary fill | `--color-fg-default` | ink | light |
| Danger fill | `--color-danger-solid` | red | red |

| Foreground | Background | Minimum | Use |
|---|---|---|---|
| `--color-fg-on-brand` | `--color-brand` | 4.5:1 | Primary label |
| `--color-fg-on-danger` | `--color-danger-solid` | 4.5:1 | Danger label |
| `--color-fg-default` | `--color-bg-subtle-hover` | 4.5:1 | Neutral label |

## Responsive behaviour

- Below 640px the primary action in a page header or form takes the full width (`block`) and secondary actions stack beneath it. The page applies `block`; the component does not change its own width on breakpoints.
- Every button has a minimum height of 44px at all sizes, including `sm`.
- At 320px a button label wraps only when the consumer removes `white-space: nowrap`; the component keeps one line, and pages stack buttons instead.

## Accessibility

### Role and pattern

Native `button` (or `a` with an `href`). No ARIA role is added. Toggle buttons use `aria-pressed` set by the consumer.

### Keyboard

| Key | Action |
|---|---|
| <kbd>Tab</kbd> | Moves focus to the button in DOM order. |
| <kbd>Enter</kbd> | Activates (also on links). |
| <kbd>Space</kbd> | Activates a `button`. |

### Focus

The global `:focus-visible` ring is visible on every variant and both themes. A busy button stays focusable and keeps focus.

### Labelling

The accessible name is the visible label. An icon-only button requires `aria-label`; icons are `aria-hidden`.

### Announcements

`busy` sets `aria-busy="true"`; the consumer announces the outcome through a toast or alert.

### Motion

Background and colour transitions and the spinner rotation are neutralised by the global reduced-motion rules (`prefers-reduced-motion` and the `w-a11y-reduce-motion` preference); the component adds no motion of its own.

## Content and internationalisation

- Verb phrase in sentence case: "Save changes", "Send request", "Remove person". Never "Click here".
- Names carry the object when the page has several: "Message Marcus".
- Translatable inputs: none (content is projected). Data values: none.

## Performance

- Change detection: `OnPush`, signal inputs, host bindings only; no template.
- No dedicated perf-test scenario exists in this repository; correctness of the host class set is covered by the unit spec, and the existing Playwright suite covers the buttons in context.
- Layout stability: the busy state keeps the button width, so a busy toggle never shifts layout.

## Acceptance criteria

### Rendering

- **AC-1** Given `<button w-button variant="primary">Save changes</button>`, when it renders, then the element has classes `w-btn` and `w-btn--primary`, keeps its text "Save changes", and has a computed height of at least 44px. (L2-NFR-010)
- **AC-2** Given each variant (`secondary`, `ghost`, `subtle`, `danger`, `link`) and `neutral`, when it renders, then the element has `w-btn--<variant>` (none for `neutral`) and no other variant class. (L2-NFR-010)
- **AC-3** Given `size="sm"` and `size="lg"`, when they render, then they carry `w-btn--sm` and `w-btn--lg` respectively, and the `sm` button is still at least 44px high. (L2-NFR-010)
- **AC-4** Given `block`, when it renders at 320px inside a card, then it is as wide as the card and the page does not scroll horizontally. (L2-NFR-009)
- **AC-5** Given `<a w-button variant="ghost" size="sm" href="/mentor/youth">Back to my youth</a>`, when it renders, then it is a link with the button classes and no underline. (L2-NFR-010)

### States

- **AC-6** Given `busy` is true, when it renders, then the element has `is-loading`, `aria-busy="true"`, the same width as before, and is not natively disabled. (L2-NFR-010)
- **AC-7** Given `busy` is true, when a user clicks or presses Enter, then no `click` handler on the element runs. (L2-NFR-010)
- **AC-8** Given a native `disabled` attribute, when it renders, then it is dimmed, not focusable, and a click does nothing. (L2-NFR-010)

### Keyboard and focus

- **AC-9** Given a page with a button, when a user tabs to it, then it shows the focus ring and Enter and Space activate a `button` (Enter activates a link). (L2-NFR-010)

### Screen readers

- **AC-10** Given `<button w-button iconOnly aria-label="Exit quiz">`, when read by a screen reader, then it is announced as "Exit quiz, button" and the icon is hidden. (L2-NFR-010)

### Theming

- **AC-11** Given the dark theme, when each variant renders, then label-on-fill contrast is at least 4.5:1. (L2-NFR-010)

### Motion

- **AC-12** Given reduced motion is on, when a button changes state, then no transition or spinner rotation plays. (L2-PLATFORM-004)

### Performance

- **AC-13** Given 360 buttons on a page, when it renders, then buttons add no wrapper elements and no template nodes. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/button/` with `w-button.ts`, `w-button.spec.ts`, `w-button.stories.ts`. selector `button[w-button], a[w-button]`; `ViewEncapsulation.None` so the host-class rules apply to the native element.
- The `busy` click guard is a capture-phase listener on the host, so it runs before consumer `(click)` handlers.
- Gaps today: apps write `class="w-btn w-btn--primary"` and bind `[class.w-btn--primary]` for dynamic variants (390 call sites). Dynamic variants become `[variant]="cond ? 'primary' : 'ghost'"`. The `link` variant has no CSS in `core.css`; the component adds it from the design-system page.
- `iconOnly` replaces `w-btn--icon`; the standalone icon control remains Icon button.

## Decisions

- **D-1** *Selector style?* Attribute selectors on `button` and `a`. This keeps native semantics, forms and `routerLink` working (DECISIONS #23).
- **D-2** *Keep the `w-btn*` classes?* Yes, on the host, as an implementation detail. Existing library and page selectors that target `.w-btn` keep working, and apps stop writing them.
- **D-3** *Busy implementation?* A capture-phase click guard, not native `disabled`, so focus stays on the button and screen readers do not lose it.
- **D-4** *Is `size="sm"` 32px or 44px?* The design-system page says 32px; the product ships 44px minimum height for every size, which L2-NFR-010 AC1 requires. The CRD keeps 44px and the design-system page should follow.
