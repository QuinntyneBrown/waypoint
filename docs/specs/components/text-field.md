# Text field

| Field | Value |
|---|---|
| Selector | `input[w-text-field]` |
| Library path | `frontend/projects/components/src/lib/text-field/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010 |
| Design system | [`text-field.html`](../../design-system/components/text-field.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) with Text field |
| Rendering | [`text-field.html`](text-field.html) |

## Purpose and scope

A text field takes one line of text. Always pair it with a visible label inside a [Form field](form-field.md).

Out of scope:

- The label, help and error text (see Form field).
- Validation: the page decides when a control is invalid and sets `aria-invalid`.

## Usage

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Sign-in, reset, invite, apply | `type` email, password, text | "Email", "Password" | default, focus, invalid | auth card, canvas |
| Admin forms and dialogs | text, number, date | "Track name", "Pass mark" | default, invalid | dialog, card |
| Filters and search | text | "Search people" | default | toolbar |

## Anatomy

1. **Control** — the native `input`, 100% wide, 1.5px strong border, rounded input radius.
2. **Value or placeholder** — body font; the placeholder is muted.

Host: the native `input` element itself; the component renders nothing of its own.

## API

### Inputs

None. All native attributes (`type`, `id`, `name`, `formControlName`, `placeholder`, `autocomplete`, `disabled`, `aria-*`) stay on the element.

### Outputs

None. Native events and Angular forms directives are the interface.

### Content slots

None. The element is void.

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
<input w-text-field class="w-input" id="email" type="email">
```

```html
<!-- consumer -->
<input w-text-field id="email" type="email" formControlName="email" autocomplete="email" />
```

## Design

- The `w-input` class stays on the host so field-level rules (`.w-field.is-error .w-input`) and page selectors keep working.
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

Native `input`; no ARIA role added.

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

- **AC-1** Given `<input w-text-field type="email">` with a label, when it renders, then it has the class `w-input`, is as wide as its container, and is at least 44px high. (L2-NFR-010)
- **AC-2** Given `placeholder="name@school.org"`, when the field is empty, then the placeholder is shown in the muted colour and is not used as the accessible name. (L2-NFR-010)

### States

- **AC-3** Given `aria-invalid="true"` inside an invalid field, when it renders, then the border and ring are red. (L2-NFR-010)

### Keyboard and focus

- **AC-4** Given a sign-in form, when a user tabs into the email input, then a visible border and ring show focus. (L2-NFR-010)

### Theming

- **AC-5** Given the dark theme, when the field has a value, then the text has contrast of at least 4.5:1 against the fill. (L2-NFR-010)

### Responsive

- **AC-6** Given a 320px viewport, when the field renders in a card, then it does not overflow the card. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/text-field/`: `w-text-field.ts`, spec, story. Selector `input[w-text-field]`; `ViewEncapsulation.None`.
- Replaces `class="w-input"` in 55 call sites. Part of one pull request with Textarea and Select, which share the combined `.w-input, .w-select, .w-textarea` rule in `core.css`.

## Decisions

- **D-1** *Cover checkbox, radio and file inputs?* No. They have their own components (Checkbox, Radio group, Switch) and the file input is not built yet.
- **D-2** *Input group addon?* `w-inputgroup` has a single use and stays local until another screen needs it.
