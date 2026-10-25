# @waypoint/components

See `frontend/README.md` for the workspace layering rules.

Every component uses the `ph` prefix in component syntax (`<ph-alert>`, `button[ph-button]`,
`[phAccessibleDialog]`, `.ph-*` classes, `--ph-*` custom properties); ESLint enforces it. Styles come
from the PromiseHub design system: `src/styles/tokens.css` is a copy of
`docs/design-system/tokens/tokens.css`, and each component's stylesheet follows its page in
`docs/design-system/components/` (DECISIONS #25). Browse the implemented components with
`npm run storybook`.
