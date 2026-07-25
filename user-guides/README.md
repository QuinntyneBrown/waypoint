# Word Up — User Guide

The end-user guide to [Word Up](../README.md): a standalone, static, no-build-step site that
documents the shipping product — the Angular portal and public site — role by role.

It is written for the people who **use** Word Up, not the people who build it: young people,
mentors, guardians, volunteers, and program administrators. Contributor documentation lives
elsewhere (see [`docs/`](../docs/), [`docs/specs/`](../docs/specs/README.md), and
[`docs/ARCHITECTURE.md`](../docs/ARCHITECTURE.md)).

The guide deploys on its own, independently of the applications. A failing application build never
blocks the documentation, and a broken page never blocks the application.

---

## How to open it

**Directly from disk.** Every page is plain HTML with no build step, no bundler, and no network
request, so it works over `file://`:

```bash
# macOS
open user-guides/index.html
# Windows
start user-guides\index.html
# Linux
xdg-open user-guides/index.html
```

**Via a local static server**, which is closer to how it is deployed:

```bash
npx --yes serve user-guides
# then open the address it prints, e.g. http://localhost:3000
```

Either way it behaves identically — links are resolved relative to each page's own depth so nothing
depends on being served from a particular root.

---

## Structure

| Path                       | Contents                                                                                                                                                           |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `index.html`               | Landing page: what Word Up does, the lifecycle figure, the who-does-what table, the card grid, reading paths per role, and what this release does and does not do. |
| `404.html`                 | Branded not-found page. Renders the full chrome and offers a way back. Deliberately **not** in the reading order.                                                  |
| `assets/guide.css`         | Every style in the guide. All values resolve through the `--wu-*` design-token contract.                                                                           |
| `assets/guide.js`          | Every behaviour in the guide, and the page registry (see below).                                                                                                   |
| `assets/vendor/core.css`   | Vendored, byte-identical copy of the Word Up design system.                                                                                                        |
| `assets/vendor/a11y.css`   | Vendored, byte-identical copy of the display/accessibility hooks.                                                                                                  |
| `start-here/`              | What you need first, opening Word Up for the first time, a named tour of the window, and one complete walkthrough.                                                 |
| `youth/`                   | Working through a lesson; taking quizzes and submitting assignments.                                                                                               |
| `mentor/`                  | Reviewing and grading work; roster, check-ins and at-risk flags.                                                                                                   |
| `administrator/`           | Applications and people; cohorts, curriculum and settings; attendance, the progress board and reports.                                                             |
| `guardian/`                | Giving consent and following a child's progress.                                                                                                                   |
| `volunteer/`               | Running today's session.                                                                                                                                           |
| `reference/`               | Messages and error codes; lifecycles and evidence; roles and permissions; accessibility and display preferences.                                                   |
| `advanced/`                | Privacy, consent and the audit trail; integration contracts; symptom-first troubleshooting.                                                                        |
| `tests/smoke.spec.ts`      | The Playwright smoke suite.                                                                                                                                        |
| `staticwebapp.config.json` | Static-host routing, MIME types, and security headers.                                                                                                             |
| `package.json`             | Pinned tooling. Private, and deliberately without a lockfile.                                                                                                      |

---

## The registry rule

**`assets/guide.js` holds a single page registry, and it is the only source of truth** for the
sidebar navigation, the breadcrumb trail, the previous/next pager, and the search index. Its array
order _is_ the reading order of the guide.

Adding a page therefore means doing exactly two things:

1. add the HTML file at `<slug>.html`, and
2. add its registry entry in `assets/guide.js`, positioned where it should be read.

There is no third place to update, and no way to update only one of the two: the smoke suite fails
in **both** directions. A registry entry with no file on disk fails
(`every page in the navigation registry exists on disk`), and a file on disk with no registry entry
fails (`every authored page on disk is registered in the navigation`).

`404.html` is the single, explicit exception, listed as such in the suite.

---

## The content rule

**The guide documents only behaviour that exists.** A guide that promises a feature the product
lacks is worse than no guide at all.

Where a control is present in the interface but not wired up — or wired to something other than what
it appears to do — the guide marks it with a **Preview limitation** callout rather than describing
it as working:

```html
<div class="wu-alert wu-alert--warning">
  <p class="wu-alert__title">Preview limitation</p>
  <p>
    What looks like it works, what actually happens, and what to do instead.
  </p>
</div>
```

Every such limitation is stated on the page where the surrounding task is described, and all of them
are collected in [`advanced/troubleshooting.html`](advanced/troubleshooting.html). The landing page
states plainly, up front, what works today and what does not — including the release-level warning
that Word Up is a development project and is **not approved for production use or for storing real
participant information**.

Every quoted button label, status word, validation message, and state name is taken **verbatim from
source**. When the product's wording changes, the guide is wrong until it is updated.

---

## Branding, tokens, and why the stylesheets are vendored

The guide is styled to look like the product, using the product's own components.

**Token rule.** Every value in `assets/guide.css` resolves through the `--wu-*` design-token
contract. There are no literal colours, type stacks, spacing values, radii, shadows, or durations in
its rule bodies — a smoke test scans the file and fails the build if one appears. Guide-only chrome
uses the `ug-` class prefix so it can never collide with the product's `wu-` components.

The guide's own dark-theme toggle is built by **remapping existing `--wu-*` tokens to one another**,
so it introduces no colour of its own. That toggle is guide chrome: the Word Up product has no
light/dark theme, and the guide says so.

**Vendoring.** The guide deploys as its own site and cannot reach a sibling directory at runtime, so
it carries **byte-identical copies** of the design-system stylesheets in `assets/vendor/`, taken
from `frontend/projects/wu-ui/src/styles/`. A gate keeps the copies honest:

```bash
# From the repository root — verify (exits non-zero on drift)
node scripts/verify-word-up-user-guide-assets.mjs

# Re-copy each drifted source over its vendored copy
node scripts/verify-word-up-user-guide-assets.mjs --fix
```

The same two commands are available from this folder as `npm run verify:assets` and
`npm run verify:assets:fix`.

`assets/vendor/` is listed in `.prettierignore`, because reformatting a vendored file would itself
be drift.

**`fonts.css` is deliberately not vendored.** It declares `@font-face` rules with absolute
`/fonts/*.woff2` URLs that cannot resolve over `file://`, and the guide is required to make no
external font request. The `--wu-font-*` tokens already carry complete system-font fallback stacks,
so omitting it degrades cleanly.

---

## Quality gates

Install once — note the flag, as this package intentionally has no lockfile:

```bash
cd user-guides
npm install --no-package-lock
```

| Command                 | Gate                                                                                                                             |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `npm run format:check`  | Prettier, default settings, over every HTML, JSON, Markdown file, both asset files, and the tests. `npm run format:write` fixes. |
| `npm run validate`      | `html-validate` over every authored page, extending its recommended configuration. Enforces `no-inline-style`.                   |
| `npm test`              | The Playwright smoke suite, line reporter, using the installed Chrome channel rather than a bundled browser.                     |
| `npm run verify:assets` | The vendored-asset drift gate described above.                                                                                   |

The smoke suite asserts that:

1. every page renders its injected chrome with **zero page errors** and makes no external request;
2. the 404 page renders the chrome and offers a working way back;
3. every page in the navigation registry **exists on disk**;
4. every authored page on disk **is registered** in the navigation;
5. **no internal link or anchor is broken**, anywhere;
6. the on-page contents links to every section on the page;
7. the pager moves forward and back through the entire reading order correctly;
8. search filters the guide and restores focus to the invoking control on `Escape`;
9. display preferences apply the token contract, and survive a reload;
10. reduced motion removes transition durations;
11. narrow viewports (320px) keep the document within the window;
12. the stylesheet contains **no hardcoded colour** outside the token contract.

`.htmlvalidate.json` turns off `doctype-style` and `void-style`. Those two rules assert the opposite
of what Prettier writes, and formatting is Prettier's job; nothing about correctness or accessibility
is relaxed.

---

## Deployment

`.github/workflows/user-guides.yml` runs two jobs, both independent of `backend.yml` and
`frontend.yml`:

- **`quality`** — the asset verifier, then dependency restore, `format:check`, `validate`, and the
  smoke suite, in that order.
- **`deploy`** — on a push to the default branch, publishes this folder as a static site **with no
  build step**. If `AZURE_STATIC_WEB_APPS_API_TOKEN` is absent the job emits a warning and skips
  rather than failing, so a fork or an unconfigured repository is never failed for a secret it was
  never given.

`staticwebapp.config.json` sets the routing, the MIME types, and these response headers:

```
content-security-policy: default-src 'self'; base-uri 'self'; form-action 'none';
  frame-ancestors 'none'; img-src 'self' data:; style-src 'self'; script-src 'self';
  font-src 'self'; connect-src 'none'; object-src 'none'
referrer-policy: no-referrer
x-content-type-options: nosniff
cross-origin-opener-policy: same-origin
```

**The consequence of that CSP is a hard authoring rule: no inline `style` attributes and no inline
`<script>` blocks anywhere in the site.** Both would be blocked by `style-src 'self'` and
`script-src 'self'`, so a page that used either would render or behave incorrectly once deployed
even though it looked fine locally. All styling lives in `assets/guide.css`, all behaviour in
`assets/guide.js`, and `html-validate` enforces the first half of the rule with `no-inline-style`.

`connect-src 'none'` likewise means the guide can never fetch anything at runtime, and
`img-src 'self' data:` means any illustration must be inline SVG or a `data:` URI. Icons in this
guide are inline SVG paths defined in `assets/guide.js` — there is no icon font and no sprite file.
