# Icon button

| Field | Value |
|---|---|
| Selector | `button[w-icon-button]`, `a[w-icon-button]` |
| Library path | `frontend/projects/components/src/lib/icon-button/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010 |
| Design system | [`icon-button.html`](../../design-system/components/icon-button.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) with Icon button (47 of 112) |
| Rendering | [`icon-button.html`](icon-button.html) |

## Purpose and scope

A round, borderless button holding a single icon. Use it when space is tight and the icon is universally understood (close, menu, notifications, more). If the meaning needs a word, use the [Button](button.md) with an icon and a label instead.

Out of scope:

- The unread dot on the notifications button. The shell draws it.
- Menus opened by the button (see Menu).

## Usage

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Dialog header | plain | `w-icon` "x", `aria-label="Close"` | default | dialog |
| Top bar | plain, `a` or `button` | menu, notifications, account icons | default | top bar |
| Password field | plain | eye icon, `aria-label` "Show password" | default, pressed | form |
| Table and list rows | plain, `a` | chevron or "open" icon | default | row |
| Editor rows | plain | drag, remove, edit, delete icons | default, disabled | card |

## Anatomy

1. **Icon** — `w-icon`, 20px, `aria-hidden`.
2. **Hit area** — 44px circle; hover fills it with `--color-bg-subtle-hover`.

Host: the native `button` or `a` itself; the component projects its content unchanged.

## API

### Inputs

None. The component has one look and size.

### Outputs

None. A native `click` event is the interface.

### Content slots

| Slot | Accepts | Rule |
|---|---|---|
| default | one `w-icon` | The consumer sets `aria-label`; the icon is decorative. |

## Variants and sizes

| Variant | Class | Use for |
|---|---|---|
| Plain | none | Every use in the product today. |

| Size | Class | Height | Padding | Type |
|---|---|---|---|---|
| md | none | `--control-height-md` | none | none |

The small (32px) size on the design-system page is not used by any screen or mock and is not built; it joins this CRD when a screen needs it.

## States

| State | Trigger | Visual | Assistive technology |
|---|---|---|---|
| Default | none | Transparent, icon in `--color-fg-default`. | Name from `aria-label`. |
| Hover | `:hover` | `--color-bg-subtle-hover` fill. | none |
| Focus | `:focus-visible` | Global focus ring. | Focus announced with name and role. |
| Disabled | native `disabled` | Not focusable, no hover fill. | Announced as unavailable. |

## Markup

```html
<!-- rendered -->
<button w-icon-button type="button" class="w-iconbtn" aria-label="Close"><svg class="w-icon" aria-hidden="true">…</svg></button>
```

```html
<!-- consumer -->
<button w-icon-button type="button" aria-label="Close" (click)="close()"><w-icon name="x" /></button>
<a w-icon-button [routerLink]="'/admin/people/' + id" aria-label="Open Priya Sharma"><w-icon name="chevron-right" /></a>
```

## Design

- The `w-iconbtn` class stays on the host so shell and dialog rules that target it keep working (`w-modal__close`, `w-topbar__burger`).
- Circle `--radius-full`, icon 20px, no border; hover uses the neutral subtle-hover fill.
- The component owns the rules; they move out of `core.css`.

Component tokens:

| Token | Aliases | Overridden by |
|---|---|---|
| None | none | none |

## Colour

| Part | Token | Light | Dark |
|---|---|---|---|
| Icon | `--color-fg-default` | ink | light |
| Hover fill | `--color-bg-subtle-hover` | ink-100 | ink-700 |

| Foreground | Background | Minimum | Use |
|---|---|---|---|
| `--color-fg-default` | `--color-bg-subtle-hover` | 3:1 | Icon on hover fill (non-text) |

## Responsive behaviour

- The button is 44 × 44 CSS px at every width and never shrinks.
- At 320px nothing scrolls horizontally or clips.

## Accessibility

### Role and pattern

Native `button` (or `a` with an `href`). No ARIA role added.

### Keyboard

| Key | Action |
|---|---|
| <kbd>Tab</kbd> | Moves focus to the button in DOM order. |
| <kbd>Enter</kbd> | Activates (links and buttons). |
| <kbd>Space</kbd> | Activates a `button`. |

### Focus

The global `:focus-visible` ring is visible in both themes.

### Labelling

An icon-only control has no visible text, so the consumer must provide `aria-label` naming the action and, where several look alike, the object: "Open Priya Sharma". The icon is `aria-hidden`.

### Announcements

None beyond the name and role.

### Motion

Hover fill has no motion of its own.

## Content and internationalisation

- Labels name the action: "Close", "Open menu", "Show password"; add the object when rows repeat: "Open Priya Sharma".
- Translatable inputs: none (content is projected). Data values: none.

## Performance

- Change detection: `OnPush`; no inputs, no template beyond `ng-content`.
- No dedicated perf-test scenario exists in this repository.
- Layout stability: fixed 44 × 44 box.

## Acceptance criteria

### Rendering

- **AC-1** Given `<button w-icon-button aria-label="Close">` containing a `w-icon`, when it renders, then the element has the class `w-iconbtn`, keeps its content, and measures 44px by 44px. (L2-NFR-010)
- **AC-2** Given `<a w-icon-button aria-label="Open Priya Sharma">`, when it renders, then it is a link with the `w-iconbtn` class and no underline. (L2-NFR-010)

### States

- **AC-3** Given a native `disabled` attribute, when it renders, then it is not focusable and a click does nothing. (L2-NFR-010)

### Keyboard and focus

- **AC-4** Given a dialog header with a Close icon button, when a user tabs to it and presses Enter, then the dialog closes and the focus ring was visible. (L2-NFR-010)

### Screen readers

- **AC-5** Given `aria-label="Close"`, when read by a screen reader, then it is announced as "Close, button" and the icon is hidden. (L2-NFR-010)

### Theming

- **AC-6** Given the dark theme, when it renders, then the icon is visible against the page and the hover fill is distinct. (L2-NFR-010)

### Responsive

- **AC-7** Given a 320px viewport with a top bar, when it renders, then every icon button stays 44 × 44 and the page does not scroll horizontally. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/icon-button/`: `w-icon-button.ts`, spec, story. Selector `button[w-icon-button], a[w-icon-button]`; `ViewEncapsulation.None` so the rules apply to the native host.
- Replaces `class="w-iconbtn"` in 14 call sites (dialog close buttons, password toggles, shell top bar).

## Decisions

- **D-1** *Build the 32px size?* No. The design-system page documents it, but no screen or mock uses it, and DECISIONS #23 builds what apps use. It is added with its first user.
- **D-2** *Inputs?* None; the component is a styled host.
