# Textarea

| Field | Value |
|---|---|
| Selector | `textarea[w-textarea]` |
| Library path | `frontend/projects/components/src/lib/textarea/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010 |
| Design system | [`textarea.html`](../../design-system/components/textarea.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) with Textarea |
| Rendering | [`textarea.html`](textarea.html) |

## Purpose and scope

A textarea takes several lines of text. Always pair it with a visible label inside a [Form field](form-field.md).

Out of scope:

- The label, help and error text (see Form field).
- Validation: the page decides when a control is invalid and sets `aria-invalid`.

## Usage

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Messages and broadcasts | `rows` 4 to 8 | "Message Marcus" | default, invalid | card |
| Reflections, assignments, notes | `rows` 3 to 6 | "Your reflection" | default, invalid | card, dialog |
| Admin forms | `rows` 3 to 4 | "Track description" | default | dialog |

## Anatomy

1. **Control** — the native `textarea`, 100% wide, 1.5px strong border, rounded input radius.
2. **Value or placeholder** — body font; the placeholder is muted.

Host: the native `textarea` element itself; the component renders nothing of its own.

## API

### Inputs

None. All native attributes (`type`, `id`, `name`, `formControlName`, `placeholder`, `autocomplete`, `disabled`, `aria-*`) stay on the element.

### Outputs

None. Native events and Angular forms directives are the interface.

### Content slots

None. Content is the value, bound by Angular forms.

## Variants and sizes

One variant and one size. Width comes from its container.

## States

| State | Trigger | Visual | Assistive technology |
|---|---|---|---|
| Default | none | Strong border on the surface fill. | Name from the label. |
| Focus | `:focus` | Ink border and a soft ring. | Focus announced with label, role and value. |
| Invalid | `aria-invalid="true"` or `.is-error` on the field | Red border and ring. | "Invalid" announced; error text read via `aria-describedby`. |
| Disabled | native `disabled` | Not focusable. | Announced as unavailable. |

## Markup

```html
<!-- rendered -->
<textarea w-textarea class="w-textarea" id="msg" rows="4"></textarea>
```

```html
<!-- consumer -->
<textarea w-textarea id="msg" rows="4" formControlName="body" maxlength="4000"></textarea>
```

## Design

- The `w-textarea` class stays on the host so field-level rules (`.w-field.is-error .w-textarea`) and page selectors keep working.
- Strong border `--border-width-hairline`, input radius, body font, 11px by 13px padding, focus ring from the ink and brand-soft tokens.
- The component owns the rules; they move out of `core.css`.

Component tokens:

| Token | Aliases | Overridden by |
|---|---|---|
| None | none | none |

## Colour

| Part | Token | Light | Dark |
|---|---|---|---|
| Border | `--color-border-strong` | ink-300 | ink-400 |
| Text | `--color-fg-default` | ink | light |
| Fill | `--color-bg-surface` | white | ink-800 |

| Foreground | Background | Minimum | Use |
|---|---|---|---|
| `--color-fg-default` | `--color-bg-surface` | 4.5:1 | Entered text |

## Responsive behaviour

- The control fills its container at every width; at 320px it never overflows.
- Minimum height keeps a 44px target.

## Accessibility

### Role and pattern

Native `textarea`; no ARIA role added.

### Keyboard

| Key | Action |
|---|---|
| <kbd>Tab</kbd> | Moves focus into and out of the control. |

### Focus

Visible border and ring on `:focus`, in both themes.

### Labelling

A visible `label` bound with `for`/`id` is required. Placeholder text is a hint, never the label.

### Announcements

Errors are linked with `aria-describedby`; the control sets `aria-invalid="true"` while invalid.

### Motion

Border and ring change over 150ms; reduced motion neutralises it globally.

## Content and internationalisation

- Placeholders show an example, not an instruction: "name@school.org".
- Translatable inputs: none (content is native attributes). Data values: the entered value.

## Performance

- Change detection: `OnPush`; host-only component with no template beyond `ng-content`.
- No dedicated perf-test scenario exists in this repository.
- Layout stability: fixed control height; no size change on focus.

## Acceptance criteria

### Rendering

- **AC-1** Given `<textarea w-textarea rows="4">`, when it renders, then it has the class `w-textarea`, a minimum height of 104px, and can be resized only vertically. (L2-NFR-010)
- **AC-2** Given a character limit of 4000, when text is entered, then the control accepts up to `maxlength` characters. (L2-NFR-010)

### States

- **AC-3** Given `aria-invalid="true"` inside an invalid field, when it renders, then the border and ring are red. (L2-NFR-010)

### Keyboard and focus

- **AC-4** Given a message form, when a user tabs into the textarea, then a visible border and ring show focus and Tab leaves the control. (L2-NFR-010)

### Theming

- **AC-5** Given the dark theme, when the textarea has text, then the text has contrast of at least 4.5:1 against the fill. (L2-NFR-010)

### Responsive

- **AC-6** Given a 320px viewport, when the textarea renders in a card, then it does not overflow the card. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/textarea/`: `w-textarea.ts`, spec, story. Selector `textarea[w-textarea]`; `ViewEncapsulation.None`. Adds a 104px minimum height and vertical-only resize.
- Replaces `class="w-textarea"` in 37 call sites.

## Decisions

- **D-1** *Auto-grow?* No. Pages set `rows`; vertical resize is allowed.
