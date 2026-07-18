# Word Up — Page Builder Contract (READ FIRST, FOLLOW EXACTLY)

> The mock pages this contract governs live in **`docs/mocks/`**. Shared assets, templates, the style guide, and the product map live at that root. Screens are organized as `<subsystem>/<role>/<page>.html`, using the same subsystem names as `docs/specs/`.

You are building **static HTML mockup pages** for the "Word Up" youth-leadership app. They must all look like one product. Follow this contract verbatim. Do **not** invent CSS classes, do **not** add colors, do **not** add external resources. Look at `styleguide.html` for the rendered look and copy its markup patterns.

## Golden rules (breaking any of these breaks the prototype)
1. **Runs from `file://` offline. ZERO network requests.** No `http://`/`https://` in any `src`/`href`. No Google Fonts link, no CDN, no analytics, no external images.
2. **No `fetch`, no `XMLHttpRequest`, no ES modules, no `import`, no `<script type="module">`.** Behavior is declarative via `data-*` attributes handled by `wordup.js`. Inline `<script>` for tiny page demos is OK but keep it minimal and classic.
3. **All filenames and hrefs lowercase ASCII**, `.html` extension. (ChromeOS is case-sensitive.)
4. **Two levels of screen nesting only.** Every screen lives directly in a role folder under its owning subsystem (e.g. `program-administration/admin/people.html`).
5. **Do not create, edit, or touch:** `assets/css/wordup.css`, `assets/js/wordup.js`, `index.html`, `styleguide.html`, other agents' folders. Create only the files assigned to you.
6. **No inline hex colors / no `style="color:#..."`.** Use component classes and tokens. (A few layout-only inline styles like `style="max-width:52ch"` or `style="--val:64%"` are fine; color must come from classes.)
7. **Use the shared sample data names** (below) so people/cohorts/lessons are consistent across every page.

## Exact `<head>` (copy this; note the `../../` for subsystem/role pages)
```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>PAGE NAME · Word Up</title>
  <link rel="icon" href="data:image/svg+xml,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 32'><rect width='32' height='32' rx='7' fill='%23c6fb50'/><text x='16' y='23' font-family='sans-serif' font-size='20' font-weight='700' text-anchor='middle' fill='%2316160c'>W</text></svg>">
  <link rel="stylesheet" href="../../assets/css/wordup.css">
  <script defer src="../../assets/js/wordup.js"></script>
</head>
```

## Page types & body
- **App pages (youth/mentor/admin/guardian/volunteer):** shell is auto-mounted. Use:
  ```html
  <body data-role="ROLE" data-active="NAVKEY" data-track="bible">
    <main class="wu-main">
      <div class="wu-container wu-page"> ... </div>
    </main>
    <!-- page-scoped modals here -->
  </body>
  ```
  `data-role` = youth | mentor | admin | guardian | volunteer. `data-active` = the nav key for this page (list below). Youth/guardian/volunteer use `.wu-container--narrow` (mobile-first). Mentor/admin use full `.wu-container`.
- **Auth pages:** no shell. Copy `../../_template-auth.html`. Wrap in `.wu-authpage` + `.wu-authcard`.
- **Public pages:** no shell. Copy `../../_template-public.html` (marketing nav + footer).
- **States pages:** no shell. Copy `../../_template-state.html`.

### Nav keys (`data-active`)
- youth: `home, journey, curriculum, badges, schedule, messages, announcements, profile`
- mentor: `dashboard, my-youth, check-ins, reviews, attendance, flags, curriculum, calendar, messages`
- admin: `dashboard, people, cohorts, applications, invitations, users-roles, cohort-progress, reports, attendance, tracks, badges, progression, calendar, announcements, settings, audit`
- guardian: `home, progress, announcements, consent`
- volunteer: `home, attendance, schedule`

## Component snippets (copy — these are the ONLY components)
**Page header**
```html
<div class="wu-page__head">
  <div class="wu-page__titles"><div class="wu-eyebrow">Section</div><h1 class="wu-page__title">Title</h1><p class="wu-page__sub">Sub.</p></div>
  <div class="wu-page__actions"><button class="wu-btn wu-btn--primary"><svg class="wu-icon"><use href="#i-plus"></use></svg>New</button></div>
</div>
```
**Icon:** `<svg class="wu-icon"><use href="#i-NAME"></use></svg>` — names: home compass book-open book users user user-check user-plus calendar bell search settings check check-circle x x-circle plus minus edit trash upload download filter sliders menu log-out log-in award bar-chart pie-chart trending-up trending-down clipboard clipboard-check message-square send chevron-right/left/down/up chevrons-right arrow-left arrow-right external-link wifi-off refresh alert-triangle alert-circle info help-circle lock mail eye eye-off star flag clock map-pin play play-circle file-text folder grid target heart shield code zap camera paperclip thumbs-up more-vertical more-horizontal sparkles graduation-cap church phone printer copy link circle activity list bookmark key video headphones layers columns table mic sun smile.
**Buttons:** `.wu-btn` + `--primary --secondary --ghost --subtle --danger --sm --lg --block --icon`. Add `is-loading` / `disabled` as needed.
**Badge:** `<span class="wu-badge wu-badge--success wu-badge--dot">On track</span>` (modifiers: success warning error info lime ink outline; `--dot` adds a leading dot). Status convention: on-track=success, in-progress=warning, at-risk=error, new=info.
**Card:** `<div class="wu-card"><div class="wu-card__header"><h3 class="wu-card__title">…</h3></div><div class="wu-card__body">…</div><div class="wu-card__footer">…</div></div>` (variants `--greige --ink --lime --flat --link`). Stat: `.wu-card.wu-statcard` › `.wu-statcard__label/__value/__delta` (`is-up`/`is-down`).
**Fields:** `.wu-field` (`is-error`) › `.wu-label` (`.wu-label__req` `*`) + `.wu-input`/`.wu-select`/`.wu-textarea` + `.wu-help` + `.wu-error-text`. Check: `<label class="wu-check"><input type="checkbox"><span class="wu-check__box"><svg class="wu-icon"><use href="#i-check"></use></svg></span><span>Label</span></label>`. Radio: `.wu-radio`/`.wu-radio__box`. Switch: `.wu-switch`/`.wu-switch__track`.
**Table:** wrap in `.wu-table-wrap`; `<table class="wu-table wu-table--cards">`; **every `<td>` needs `data-label="Column"`** for mobile reflow. First cell can use `<div class="wu-table__cellmain"><span class="wu-avatar wu-avatar--sm" data-name="Full Name"></span>Name</div>`.
**Avatar:** `<span class="wu-avatar" data-name="Full Name"></span>` (sizes `--xs --sm --lg --xl`). Initials + color auto-filled by JS. Person block: `.wu-person`/`.wu-person__name`/`.wu-person__meta`.
**Progress ring:** `<div class="wu-ring" style="--val:64"><span class="wu-ring__label">64%</span></div>` (sizes `--sm --lg`). **Bar:** `<div class="wu-progress"><div class="wu-progress__fill" style="--val:64%"></div></div>` (note bar uses `%`, ring uses number). Row: `.wu-progress__row` with `.wu-progress__num`.
**Tabs:** wrap in `[data-tabs-root]`; `<div class="wu-tabs"><button class="wu-tab is-active" data-tab="a">…</button>…</div>` + `<div data-tabpanel="a" class="wu-tabpanel is-active">…</div>`.
**Alert:** `<div class="wu-alert wu-alert--info"><svg class="wu-alert__icon wu-icon"><use href="#i-info"></use></svg><div><div class="wu-alert__title">Title</div>Body</div></div>` (success warning error info lime).
**Empty state:** `<div class="wu-empty"><div class="wu-empty__icon"><svg class="wu-icon"><use href="#i-…"></use></svg></div><h3 class="wu-empty__title">…</h3><p class="wu-empty__text">…</p><button class="wu-btn wu-btn--primary">…</button></div>`.
**Skeleton:** `<span class="wu-skeleton wu-skeleton--title"></span>` / `--text` / `--block` / `--circle` / `--btn`.
**Scripture block:** `<div class="wu-scripture">“…”<span class="wu-scripture__ref">Mark 10:45</span></div>`.
**Timeline:** `.wu-timeline` › `.wu-timeline__item` (`is-done`/`is-current`). **Lesson step:** `.wu-lessonstep` (`is-done`/`is-locked`) › `.wu-lessonstep__num`. **Quiz option:** `.wu-quiz__opt` (`is-selected`/`is-correct`/`is-wrong`) › `.wu-quiz__key`. **Badge medal:** `.wu-badgeitem` (`is-locked`) › `.wu-badgeitem__medal`. **Progress matrix (spreadsheet grid):** `.wu-progressmatrix` > `table.wu-matrix`; cells `.wu-cell.wu-cell--done/--prog/--none/--na`.
**Layout helpers:** `.wu-stack`(--sm/--lg) `.wu-cluster`(--between) `.wu-grid`(--2/--3/--4/--auto) `.wu-divider` `.wu-muted` `.wu-eyebrow`. Segmented control: `.wu-segment`/`.wu-segment__item`.

## Behavior hooks (declarative — no custom JS needed)
- Open a modal: `data-open-modal="m-id"`. Close: `data-close-modal` (optionally `="m-id"`).
- Fire a toast on click: `data-toast="Saved" data-toast-type="success"` (types: success error warning info) optional `data-toast-title="…"`, optional `data-then="learning-experience/youth/home.html"` to navigate after. `data-then` is relative to the `docs/mocks/` root because the shared runtime resolves it through `WordUp.href()`.
- Track switch: `data-track-switch="coding"`. Offline toggle: `data-toggle-offline`.
- Cross-page links: use normal paths relative to the current HTML file (e.g. `href="lesson.html"` within the same subsystem/role folder, or `../../mentoring/mentor/dashboard.html` across subsystems). Shell links are handled for you.

## Modal markup (page-scoped, placed before `</body>`)
```html
<div class="wu-modal wu-modal--sm" id="m-ACTION" role="dialog" aria-modal="true">
  <div class="wu-modal__scrim"></div>
  <div class="wu-modal__panel">
    <div class="wu-modal__header"><div><h2 class="wu-modal__title">Title</h2><p class="wu-modal__sub">Optional sub.</p></div>
      <button class="wu-iconbtn wu-modal__close" data-close-modal aria-label="Close"><svg class="wu-icon"><use href="#i-x"></use></svg></button></div>
    <div class="wu-modal__body"> … form/content … </div>
    <div class="wu-modal__footer"><button class="wu-btn wu-btn--ghost" data-close-modal>Cancel</button>
      <button class="wu-btn wu-btn--primary" data-close-modal data-toast="Done" data-toast-type="success">Confirm</button></div>
  </div>
</div>
```
Destructive confirms: use `wu-modal__icon wu-modal__icon--danger` + `.wu-btn--danger`.

## `?state=` and `?open=` coverage (IMPORTANT — this is how empty/loading/error get reviewed)
On list/data pages, include the normal content AND alternate blocks toggled by the `state` URL param. Wrap each in `data-when-state`:
```html
<div data-when-state="default">…the populated page…</div>
<div data-when-state="loading" class="wu-hide">…skeletons…</div>
<div data-when-state="empty" class="wu-hide">…empty state…</div>
<div data-when-state="error" class="wu-hide">…error alert + retry…</div>
```
`wordup.js` shows the block matching `?state=` (default = `default`) and hides the rest. The hub links to `page.html?state=empty`, `?state=loading`, `?open=m-invite`, etc. So: give each list page a `loading` (skeletons) and `empty` block, and give forms/actions their modals with ids. `?open=m-id` auto-opens that modal on load.

## Shared sample data (use these names EXACTLY for consistency)
- **Org / director:** Word Up · Toronto · Director = **Shawn Peters**.
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
