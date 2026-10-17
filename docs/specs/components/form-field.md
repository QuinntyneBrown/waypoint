# Form field

| Field | Value |
|---|---|
| Selector | `div[w-form-field]`, `label[w-label]`, `span[w-label-required]`, `span[w-label-optional]`, `p[w-help]`, `p[w-error-text]` |
| Library path | `frontend/projects/components/src/lib/form-field/` |
| Status | built |
| Traces to | L2-NFR-009, L2-NFR-010 |
| Design system | [`form-field.html`](../../design-system/components/form-field.html) |
| Source mocks | every screen listed in [`screen-composition.md`](../../screen-composition.md) with Form field (60 of 112) |
| Rendering | [`form-field.html`](form-field.html) |

## Purpose and scope

A form field groups a control with its label, optional help text and error message, so the three stay associated and spaced consistently. The control itself is the [Text field](text-field.md), [Textarea](textarea.md), [Select](select.md), [Checkbox](checkbox.md), [Radio group](radio-group.md) or [Switch](switch.md).

The component is five small parts used together: the field container, the label (with required and optional markers), help text and error text.

Out of scope:

- Validation rules and when an error is shown. The page decides and renders the error.
- Multi-column form layout (see Form layout, not built yet).

## Usage

| Where | Configuration | Slots / content | States seen | Surface |
|---|---|---|---|---|
| Sign-in, reset, invite and apply forms | container + label + control + error | "Password", "Enter your password." | default, error | auth card, canvas |
| Admin and mentor dialogs | container + label + control + error | "Track name", "Enter a cohort name." | default, error | dialog |
| Youth assignment and reflection | label + control + hint + error | "Written response", "JPG or PNG, up to 10 MB." | default, error | card |
| Toolbars and filters | container with no bottom margin | "Search people" | default | toolbar |
| Required and optional fields | label with a marker | "Email *", "Church (optional)" | default | any form |

## Anatomy

1. **Container** — block with a bottom margin; `div[w-form-field]`.
2. **Label** — bold, above the control; `label[w-label]` bound to the control with `for`.
3. **Marker** (optional) — `span[w-label-required]` (a red "*") or `span[w-label-optional]` ("(optional)", muted).
4. **Control** — a library control component.
5. **Help text** (optional) — muted caption under the control; `p[w-help]`.
6. **Error text** (optional) — red caption under the control; `p[w-error-text]`, rendered only while the field is invalid.

Host: each part is the native element it is applied to; content is projected unchanged.

## API

### Inputs

| Input | Type | Default | Required | Rule |
|---|---|---|---|---|
| `invalid` (on `w-form-field`) | `boolean` | `false` | no | Marks the field in error: control border and ring turn red. |

The other parts have no inputs.

### Outputs

None.

### Content slots

| Slot | Accepts | Rule |
|---|---|---|
| default (field) | label, control, help, error | In that order. The control carries `id`, `aria-invalid` and `aria-describedby`. |
| default (label) | text, optional marker | The text names the control. |

## Variants and sizes

| Variant | Class | Use for |
|---|---|---|
| Field | `w-field` | One control with its label. |
| Field without bottom margin | consumer spacing utility | Toolbars and filter rows. |

One size.

## States

| State | Trigger | Visual | Assistive technology |
|---|---|---|---|
| Default | none | Label above control. | Label read with the control. |
| Help | help text present | Muted caption. | Linked with `aria-describedby`. |
| Error | `invalid` | Red border and ring on the control; red error text. | `aria-invalid="true"` on the control and the error text in `aria-describedby`. |

## Markup

```html
<!-- rendered: invalid field -->
<div w-form-field class="w-field is-error">
  <label w-label class="w-label" for="email">Email <span w-label-required class="w-label__req">*</span></label>
  <input id="email" type="email" aria-invalid="true" aria-describedby="email-e">
  <p w-error-text class="w-error-text" id="email-e">Enter a valid email address.</p>
</div>
```

```html
<!-- consumer -->
<div w-form-field [invalid]="form.controls.email.touched && form.controls.email.invalid">
  <label w-label for="email">Email <span w-label-required>*</span></label>
  <input w-text-field id="email" type="email" formControlName="email" />
  @if (form.controls.email.touched && form.controls.email.invalid) {
    <p w-error-text id="email-e">Enter a valid email address.</p>
  }
</div>
```

## Design

- Container block, bottom margin `--space-5`; label weight 600, 0.85rem, 6px above the control; help and error 0.8rem, 5px below.
- Error text is displayed whenever it is rendered. Pages render it conditionally; the previous rule that hid it unless the field carried `is-error` is removed so one component covers every error message in the product.
- The `w-field`, `w-label`, `w-label__req`, `w-label__opt`, `w-help` and `w-error-text` classes stay on the hosts; the control rules for `.is-error` stay with the field.
- The component owns the rules; they move out of `core.css`.

Component tokens:

| Token | Aliases | Overridden by |
|---|---|---|
| None | none | none |

## Colour

| Part | Token | Light | Dark |
|---|---|---|---|
| Label | `--color-fg-default` | ink | light |
| Help | `--color-fg-muted` | ink-600 | ink-300 |
| Error text | `--color-danger-fg` | red | red-light |
| Required marker | `--color-danger-fg` | red | red-light |

| Foreground | Background | Minimum | Use |
|---|---|---|---|
| `--color-fg-muted` | `--color-bg-surface` | 4.5:1 | Help text |
| `--color-danger-fg` | `--color-bg-surface` | 4.5:1 | Error text |

## Responsive behaviour

- Fields stack in a single column; multi-column placement belongs to the page layout.
- At 320px labels and messages wrap; nothing scrolls horizontally or clips.

## Accessibility

### Role and pattern

Native `label` bound to its control with `for`/`id`. Error and help text are linked to the control with `aria-describedby`; the control carries `aria-invalid="true"` while invalid. The components add no ARIA of their own.

### Keyboard

| Key | Action |
|---|---|
| <kbd>Tab</kbd> | Moves to the control; labels are not tab stops. |

### Focus

Focus stays on the control; the container never takes focus.

### Labelling

Every control has a visible label. The required marker is decorative: the control itself has the `required` attribute so assistive technology announces it.

### Announcements

The error text is in the control's description, so it is read when the control receives focus. Pages that need an immediate announcement use `role="alert"` on the error text.

### Motion

None.

## Content and internationalisation

- Labels are nouns: "Email", "Track name". Errors say how to fix it: "Enter a valid email address."
- "(optional)" is shown on optional fields in forms where most fields are required.
- Translatable inputs: none (content is projected). Data values: none.

## Performance

- Change detection: `OnPush`; host-only components with no template beyond `ng-content`.
- No dedicated perf-test scenario exists in this repository.
- Layout stability: an error message adds height below the control; pages reserve nothing, so the next field moves down by one line.

## Acceptance criteria

### Rendering

- **AC-1** Given `<div w-form-field>` with a label, input and help text, when it renders, then the container has `w-field`, the label has `w-label` and sits above the input, and the help text sits below it. (L2-NFR-010)
- **AC-2** Given `<span w-label-required>*</span>` inside a label, when it renders, then the marker is red and the control it labels still has its own `required` attribute. (L2-NFR-010)

### States

- **AC-3** Given `invalid` is true, when the field renders, then the container has `is-error` and the input shows the red border and ring. (L2-NFR-010)
- **AC-4** Given an error message rendered under a field, when the page shows it, then it is visible whether or not the container is marked invalid. (L2-NFR-010)

### Keyboard and focus

- **AC-5** Given a sign-in form, when a user presses Tab, then focus moves from the email input to the password input without stopping on any label or message. (L2-NFR-010)

### Screen readers

- **AC-6** Given an invalid email input whose `aria-describedby` names the error text, when the input receives focus, then the screen reader announces the label, "invalid" and "Enter a valid email address." (L2-NFR-010)

### Theming

- **AC-7** Given the dark theme, when a field with help and an error renders, then help text and error text each have contrast of at least 4.5:1. (L2-NFR-010)

### Responsive

- **AC-8** Given a 320px viewport and the label "Evidence link (optional)", when the field renders, then the label wraps and the page does not scroll horizontally. (L2-NFR-009)

## Implementation notes

- Built in `frontend/projects/components/src/lib/form-field/w-form-field.ts` with directives `WFormField`, `WLabel`, `WLabelRequired`, `WLabelOptional`, `WHelp`, `WErrorText`; spec and story alongside. All use `ViewEncapsulation.None`.
- Replaces about 230 `w-field`, 250 `w-label`, 45 `w-help`/`w-field__hint`/`w-hint`, and 60 `w-error-text`/`w-field__error`/`w-error` call sites. Eight pre-existing class names for the same three parts become these directives; most had no CSS and now render in the design-system style.
- `[class.is-error]` bindings on fields become `[invalid]`.

## Decisions

- **D-1** *Error text visibility?* Always shown when rendered. Pages already render it with `@if`; 38 call sites used unstyled class names that were visible, so hiding by default would have lost messages.
- **D-2** *One directive per part?* Yes. Each part is a distinct native element with its own style; a single wrapper component would force the page to restructure its markup.
- **D-3** *Required marker?* A separate span directive, not a `required` input on the label, so existing markup (a span inside the label) converts without changing the label text.
