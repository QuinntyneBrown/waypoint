# Component requirements

One requirements document (CRD) and one rendering per component of `@waypoint/components` (DECISIONS #23). Each CRD is written just before its component is built. Screen-to-component mapping is in [`../../screen-composition.md`](../../screen-composition.md).

| Component | Selector | Status | L2 trace | Requirements | Rendering | Design system |
|---|---|---|---|---|---|---|
| Button | `button[w-button]`, `a[w-button]` | built | L2-NFR-009, L2-NFR-010, L2-PLATFORM-004 | [button.md](button.md) | [button.html](button.html) | [button](../../design-system/components/button.html) |
| Form field | `div[w-form-field]`, `label[w-label]`, `span[w-label-required]`, `span[w-label-optional]`, `p[w-help]`, `p[w-error-text]` | built | L2-NFR-009, L2-NFR-010 | [form-field.md](form-field.md) | [form-field.html](form-field.html) | [form-field](../../design-system/components/form-field.html) |
| Icon button | `button[w-icon-button]`, `a[w-icon-button]` | built | L2-NFR-009, L2-NFR-010 | [icon-button.md](icon-button.md) | [icon-button.html](icon-button.html) | [icon-button](../../design-system/components/icon-button.html) |
| Link | `a[w-link]` | built | L2-NFR-009, L2-NFR-010 | [link.md](link.md) | [link.html](link.html) | [link](../../design-system/components/link.html) |
| Select | `select[w-select]` | built | L2-NFR-009, L2-NFR-010 | [select.md](select.md) | [select.html](select.html) | [select](../../design-system/components/select.html) |
| Text field | `input[w-text-field]` | built | L2-NFR-009, L2-NFR-010 | [text-field.md](text-field.md) | [text-field.html](text-field.html) | [text-field](../../design-system/components/text-field.html) |
| Textarea | `textarea[w-textarea]` | built | L2-NFR-009, L2-NFR-010 | [textarea.md](textarea.md) | [textarea.html](textarea.html) | [textarea](../../design-system/components/textarea.html) |
