# Waypoint — Page Builder Contract (READ FIRST, FOLLOW EXACTLY)

> The mock pages this contract governs live in **`docs/mocks/`**. Shared assets, templates, the style guide, and the product map live at that root. Screens are organized as `<subsystem>/<role>/<page>.html`, using the same subsystem names as `docs/specs/`.

You are building **static HTML mockup pages** for the "Waypoint" youth-leadership app. They must all look like one product. Follow this contract verbatim. Do **not** invent CSS classes, do **not** add colors, do **not** add external resources. Look at the design system (`docs/design-system/index.html`) for the rendered look and copy markup from each component page's Code section.

## Golden rules (breaking any of these breaks the prototype)
1. **Runs from `file://` offline. ZERO network requests.** No `http://`/`https://` in any `src`/`href`. No Google Fonts link, no CDN, no analytics, no external images.
2. **No `fetch`, no `XMLHttpRequest`, no ES modules, no `import`, no `<script type="module">`.** Behavior is declarative via `data-*` attributes handled by `waypoint.js`. Inline `<script>` for tiny page demos is OK but keep it minimal and classic.
3. **All filenames and hrefs lowercase ASCII**, `.html` extension. (ChromeOS is case-sensitive.)
4. **Two levels of screen nesting only.** Every screen lives directly in a role folder under its owning subsystem (e.g. `program-administration/admin/people.html`).
5. **Do not create, edit, or touch:** `docs/design-system/` (tokens and component CSS), `assets/js/waypoint.js`, `index.html`, other agents' folders. Create only the files assigned to you.
6. **No inline hex colors / no `style="color:#..."`.** Use component classes and tokens. (A few layout-only inline styles like `style="max-width:52ch"` or `style="--val:64%"` are fine; color must come from classes.)
7. **Use the shared sample data names** (below) so people/cohorts/lessons are consistent across every page.

## Exact `<head>` (copy this; note the `../../` for subsystem/role pages)
```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>PAGE NAME · Waypoint</title>
  <link rel="icon" href="data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><rect width='32' height='32' rx='7' fill='%23c6fb50'/><text x='16' y='23' font-family='sans-serif' font-size='20' font-weight='700' text-anchor='middle' fill='%2316160c'>W</text></svg>">
  <link rel="stylesheet" href="../../../design-system/assets/fonts.css">
  <link rel="stylesheet" href="../../../design-system/tokens/tokens.css">
  <link rel="stylesheet" href="../../../design-system/assets/components.css">
  <script defer src="../../assets/js/waypoint.js"></script>
</head>
```

## Page types & body
- **App pages (youth/mentor/admin/guardian/volunteer):** shell is auto-mounted. Use:
  ```html
  <body data-role="ROLE" data-active="NAVKEY" data-track="bible">
    <main class="w-main">
      <div class="w-container w-page"> ... </div>
    </main>
    <!-- page-scoped modals here -->
  </body>
  ```
  `data-role` = youth | mentor | admin | guardian | volunteer | platform. `data-active` = the nav key for this page (list below). Youth/guardian/volunteer use `.w-container--narrow` (mobile-first). Mentor/admin use full `.w-container`.
- **Auth pages:** no shell. Copy `../../_template-auth.html`. Wrap in `.w-authpage` + `.w-authcard`.
- **Public pages:** no shell. Copy `../../_template-public.html` (marketing nav + footer).
- **States pages:** no shell. Copy `../../_template-state.html`.

### Nav keys (`data-active`)
- youth: `home, journey, curriculum, badges, schedule, messages, announcements, profile`
- mentor: `dashboard, my-youth, check-ins, reviews, attendance, flags, curriculum, calendar, messages`
- admin: `dashboard, people, cohorts, applications, invitations, users-roles, cohort-progress, reports, attendance, tracks, badges, progression, calendar, announcements, settings, audit`
- guardian: `home, progress, announcements, consent`
- volunteer: `home, attendance, schedule`
- platform: `organizations`

## Component snippets (copy — these are the ONLY components)
**Page header**
```html
<div class="w-page__head">
  <div class="w-page__titles"><div class="w-eyebrow">Section</div><h1 class="w-page__title">Title</h1><p class="w-page__sub">Sub.</p></div>
  <div class="w-page__actions"><button class="w-btn w-btn--primary"><svg class="w-icon"><use href="#i-plus"></use></svg>New</button></div>
</div>
```
**Icon:** `<svg class="w-icon"><use href="#i-NAME"></use></svg>` — names: home compass book-open book users user user-check user-plus calendar bell search settings check check-circle x x-circle plus minus edit trash upload download filter sliders menu log-out log-in award bar-chart pie-chart trending-up trending-down clipboard clipboard-check message-square send chevron-right/left/down/up chevrons-right arrow-left arrow-right external-link wifi-off refresh alert-triangle alert-circle info help-circle lock mail eye eye-off star flag clock map-pin play play-circle file-text folder grid target heart shield code zap camera paperclip thumbs-up more-vertical more-horizontal sparkles graduation-cap church phone printer copy link circle activity list bookmark key video headphones layers columns table mic sun smile.
**Buttons:** `.w-btn` + `--primary --secondary --ghost --subtle --danger --sm --lg --block --icon`. Add `is-loading` / `disabled` as needed.
**Badge:** `<span class="w-badge w-badge--success w-badge--dot">On track</span>` (modifiers: success warning error info lime ink outline; `--dot` adds a leading dot). Status convention: on-track=success, in-progress=warning, at-risk=error, new=info.
**Card:** `<div class="w-card"><div class="w-card__header"><h3 class="w-card__title">…</h3></div><div class="w-card__body">…</div><div class="w-card__footer">…</div></div>` (variants `--greige --ink --lime --flat --link`). Stat: `.w-card.w-statcard` › `.w-statcard__label/__value/__delta` (`is-up`/`is-down`).
**Fields:** `.w-field` (`is-error`) › `.w-label` (`.w-label__req` `*`) + `.w-input`/`.w-select`/`.w-textarea` + `.w-help` + `.w-error-text`. Check: `<label class="w-check"><input type="checkbox"><span class="w-check__box"><svg class="w-icon"><use href="#i-check"></use></svg></span><span>Label</span></label>`. Radio: `.w-radio`/`.w-radio__box`. Switch: `.w-switch`/`.w-switch__track`.
**Table:** wrap in `.w-table-wrap`; `<table class="w-table w-table--cards">`; **every `<td>` needs `data-label="Column"`** for mobile reflow. First cell can use `<div class="w-table__cellmain"><span class="w-avatar w-avatar--sm" data-name="Full Name"></span>Name</div>`.
**Avatar:** `<span class="w-avatar" data-name="Full Name"></span>` (sizes `--xs --sm --lg --xl`). Initials + color auto-filled by JS. Person block: `.w-person`/`.w-person__name`/`.w-person__meta`.
**Progress ring:** `<div class="w-ring" style="--val:64"><span class="w-ring__label">64%</span></div>` (sizes `--sm --lg`). **Bar:** `<div class="w-progress"><div class="w-progress__fill" style="--val:64%"></div></div>` (note bar uses `%`, ring uses number). Row: `.w-progress__row` with `.w-progress__num`.
**Tabs:** wrap in `[data-tabs-root]`; `<div class="w-tabs"><button class="w-tab is-active" data-tab="a">…</button>…</div>` + `<div data-tabpanel="a" class="w-tabpanel is-active">…</div>`.
**Alert:** `<div class="w-alert w-alert--info"><svg class="w-alert__icon w-icon"><use href="#i-info"></use></svg><div><div class="w-alert__title">Title</div>Body</div></div>` (success warning error info lime).
**Empty state:** `<div class="w-empty"><div class="w-empty__icon"><svg class="w-icon"><use href="#i-…"></use></svg></div><h3 class="w-empty__title">…</h3><p class="w-empty__text">…</p><button class="w-btn w-btn--primary">…</button></div>`.
**Skeleton:** `<span class="w-skeleton w-skeleton--title"></span>` / `--text` / `--block` / `--circle` / `--btn`.
**Scripture block:** `<div class="w-scripture">“…”<span class="w-scripture__ref">Mark 10:45</span></div>`.
**Timeline:** `.w-timeline` › `.w-timeline__item` (`is-done`/`is-current`). **Lesson step:** `.w-lessonstep` (`is-done`/`is-locked`) › `.w-lessonstep__num`. **Quiz option:** `.w-quiz__opt` (`is-selected`/`is-correct`/`is-wrong`) › `.w-quiz__key`. **Badge medal:** `.w-badgeitem` (`is-locked`) › `.w-badgeitem__medal`. **Progress matrix (spreadsheet grid):** `.w-progressmatrix` > `table.w-matrix`; cells `.w-cell.w-cell--done/--prog/--none/--na`.
**Layout helpers:** `.w-stack`(--sm/--lg) `.w-cluster`(--between) `.w-grid`(--2/--3/--4/--auto) `.w-divider` `.w-muted` `.w-eyebrow`. Segmented control: `.w-segment`/`.w-segment__item`.

## Behavior hooks (declarative — no custom JS needed)
- Open a modal: `data-open-modal="m-id"`. Close: `data-close-modal` (optionally `="m-id"`).
- Fire a toast on click: `data-toast="Saved" data-toast-type="success"` (types: success error warning info) optional `data-toast-title="…"`, optional `data-then="learning-experience/youth/home.html"` to navigate after. `data-then` is relative to the `docs/mocks/` root because the shared runtime resolves it through `Waypoint.href()`.
- Track switch: `data-track-switch="coding"`. Offline toggle: `data-toggle-offline`.
- Cross-page links: use normal paths relative to the current HTML file (e.g. `href="lesson.html"` within the same subsystem/role folder, or `../../mentoring/mentor/dashboard.html` across subsystems). Shell links are handled for you.

## Modal markup (page-scoped, placed before `</body>`)
```html
<div class="w-modal w-modal--sm" id="m-ACTION" role="dialog" aria-modal="true">
  <div class="w-modal__scrim"></div>
  <div class="w-modal__panel">
    <div class="w-modal__header"><div><h2 class="w-modal__title">Title</h2><p class="w-modal__sub">Optional sub.</p></div>
      <button class="w-iconbtn w-modal__close" data-close-modal aria-label="Close"><svg class="w-icon"><use href="#i-x"></use></svg></button></div>
    <div class="w-modal__body"> … form/content … </div>
    <div class="w-modal__footer"><button class="w-btn w-btn--ghost" data-close-modal>Cancel</button>
      <button class="w-btn w-btn--primary" data-close-modal data-toast="Done" data-toast-type="success">Confirm</button></div>
  </div>
</div>
```
Destructive confirms: use `w-modal__icon w-modal__icon--danger` + `.w-btn--danger`.

## `?state=` and `?open=` coverage (IMPORTANT — this is how empty/loading/error get reviewed)
On list/data pages, include the normal content AND alternate blocks toggled by the `state` URL param. Wrap each in `data-when-state`:
```html
<div data-when-state="default">…the populated page…</div>
<div data-when-state="loading" class="w-hide">…skeletons…</div>
<div data-when-state="empty" class="w-hide">…empty state…</div>
<div data-when-state="error" class="w-hide">…error alert + retry…</div>
```
`waypoint.js` shows the block matching `?state=` (default = `default`) and hides the rest. The hub links to `page.html?state=empty`, `?state=loading`, `?open=m-invite`, etc. So: give each list page a `loading` (skeletons) and `empty` block, and give forms/actions their modals with ids. `?open=m-id` auto-opens that modal on load.

## Shared sample data (use these names EXACTLY for consistency)
- **Org / director:** Waypoint · Toronto · Director = **Shawn Peters**.
- **Mentors:** Marcus Bello (Lead Mentor), Grace Adeyemi, Danielle Osei.
- **Youth:** Jayden Alcantara (82%, on-track), Amara Okafor (74%), Devon Grant (38%, at-risk), Priya Sharma (91%), Malik Johnson (55%), Sofia Nguyen (12%, new), Elijah Brown (47%), Fatima Ali (63%), Noah Williams (29%, at-risk), Zara Mensah (70%), Tyrone Clarke (5%, new), Leila Haddad (44%).
- **Cohorts:** Rexdale Fall '25 (Sep–Dec 2025, mentor Marcus Bello), Warden Winter '26 (Jan–Apr 2026, Danielle Osei), St. Clair Spring '26 (Apr–Jul 2026, Grace Adeyemi).
- **Tracks:** "Bible & Leadership" (bible), "Coding for Kingdom" (coding).
- **Bible modules:** Who Is Jesus?, Identity in Christ, Servant Leadership, Leading a Small Group, Sharing Your Story. **Coding modules:** Intro to Scratch, Web Basics: HTML & CSS.
- **Sample lessons (Servant Leadership):** "The Towel and the Basin" (John 13), "Last Shall Be First" (Mark 10), "Leading Like Jesus". **Scripture anchor:** Mark 10:45.
- **Badges:** First Steps, Word Warrior, Faithful Five, Servant Heart, Storyteller, Code Starter.
- Today's date context: **July 2026**.

## Voice
Warm, encouraging, plainspoken, action-oriented — purposeful but not churchy-kitsch. Youth-facing copy is short and low-reading-load. Mirror FaithTech's confident, modern, mission-driven tone (build, belong, grow).

## Accessibility
44px+ tap targets (already in components), real `<label>`s, don't rely on color alone (status badges include a dot + text), `alt`/`aria-label` on icon-only buttons.
