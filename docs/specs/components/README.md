# Component requirements

One requirements document (CRD) and one rendering per component of `@waypoint/components` (DECISIONS #23). Each CRD is written just before its component is built. Screen-to-component mapping is in [`../../screen-composition.md`](../../screen-composition.md).

| Component | Selector | Status | L2 trace | Requirements | Rendering | Design system |
|---|---|---|---|---|---|---|
| Button | `button[w-button]`, `a[w-button]` | built | L2-NFR-009, L2-NFR-010, L2-PLATFORM-004 | [button.md](button.md) | [button.html](button.html) | [button](../../design-system/components/button.html) |
| Icon button | `button[w-icon-button]`, `a[w-icon-button]` | built | L2-NFR-009, L2-NFR-010 | [icon-button.md](icon-button.md) | [icon-button.html](icon-button.html) | [icon-button](../../design-system/components/icon-button.html) |
