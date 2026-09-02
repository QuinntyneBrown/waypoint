/**
 * Smoke suite for the Waypoint user guide.
 *
 * The guide has no build step and no server, so every spec loads pages straight
 * off disk over `file://` — the same way a reader can open them.
 *
 * The two registry tests ("every registered page exists on disk" and "every page
 * on disk is registered") are the gate that keeps `assets/guide.js` and the file
 * tree in agreement. Adding a page means adding both; these tests fail if you
 * add only one.
 */
import { expect, test, type Page } from "@playwright/test";
import { readFileSync, readdirSync, statSync } from "node:fs";
import { dirname, join, posix, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const GUIDE_ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..");

/** Directories that hold authored guide pages. Everything else is tooling. */
const PAGE_DIRS = [
  "start-here",
  "youth",
  "mentor",
  "administrator",
  "guardian",
  "volunteer",
  "reference",
  "advanced",
];

/** Pages that are intentionally not in the reading order. */
const UNREGISTERED_BY_DESIGN = new Set(["404"]);

interface RegistryEntry {
  category: string;
  slug: string;
  title: string;
  icon: string;
  description: string;
  keywords: string[];
}

function urlFor(slug: string): string {
  return pathToFileURL(join(GUIDE_ROOT, `${slug}.html`)).href;
}

/** Every `.html` file on disk that is meant to be a guide page, as a slug. */
function authoredSlugs(): string[] {
  const slugs: string[] = [];

  for (const entry of readdirSync(GUIDE_ROOT)) {
    if (entry.endsWith(".html")) slugs.push(entry.replace(/\.html$/, ""));
  }

  for (const dir of PAGE_DIRS) {
    const abs = join(GUIDE_ROOT, dir);
    let entries: string[] = [];
    try {
      if (!statSync(abs).isDirectory()) continue;
      entries = readdirSync(abs);
    } catch {
      continue;
    }
    for (const entry of entries) {
      if (entry.endsWith(".html"))
        slugs.push(`${dir}/${entry.replace(/\.html$/, "")}`);
    }
  }

  return slugs.sort();
}

/**
 * The registry, read straight out of `assets/guide.js`.
 *
 * Deliberately parsed in Node rather than read from a running page: every spec
 * needs it, and making each worker launch a browser just to fetch it made the
 * suite slow and intermittently flaky (Chrome aborts a `file://` navigation
 * when several workers start together). The first spec still asserts that the
 * *running* script exposes exactly this registry, so the two cannot diverge.
 */
function readRegistry(): RegistryEntry[] {
  const source = readFileSync(join(GUIDE_ROOT, "assets", "guide.js"), "utf8");

  const declaration = source.indexOf("var PAGES = [");
  if (declaration === -1) {
    throw new Error("Could not find `var PAGES = [` in assets/guide.js.");
  }

  const start = source.indexOf("[", declaration);
  const end = source.indexOf("\n  ];", start);
  if (end === -1) {
    throw new Error(
      "Could not find the end of the PAGES array in assets/guide.js.",
    );
  }

  const literal = source.slice(start, end + "\n  ]".length);
  return new Function(`return ${literal};`)() as RegistryEntry[];
}

/**
 * Opens a guide page.
 *
 * Chrome intermittently aborts a `file://` navigation with `net::ERR_ABORTED`,
 * most often on the first load into a fresh context. That is a browser loading
 * quirk rather than anything wrong with the page, so it is retried here — and
 * only here, so a genuine assertion failure is never retried away.
 */
async function visit(page: Page, slug: string): Promise<void> {
  let lastError: unknown;

  for (let attempt = 0; attempt < 3; attempt += 1) {
    try {
      await page.goto(urlFor(slug), { waitUntil: "load" });
      return;
    } catch (error) {
      if (!String(error).includes("ERR_ABORTED")) throw error;
      lastError = error;
      await page.waitForTimeout(200 * (attempt + 1));
    }
  }

  throw lastError;
}

/** Fails the test if the page logged an error while loading. */
function trackPageErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on("pageerror", (error) => errors.push(`pageerror: ${error.message}`));
  page.on("console", (message) => {
    if (message.type() === "error") errors.push(`console: ${message.text()}`);
  });
  return errors;
}

const registry: RegistryEntry[] = readRegistry();

test("the registry is readable and not empty", () => {
  expect(registry.length).toBeGreaterThan(0);
  for (const entry of registry) {
    expect(entry.slug, JSON.stringify(entry)).toBeTruthy();
    expect(entry.title, entry.slug).toBeTruthy();
    expect(entry.category, entry.slug).toBeTruthy();
    expect(entry.description, entry.slug).toBeTruthy();
    expect(entry.keywords.length, entry.slug).toBeGreaterThan(0);
  }
});

/* ==========================================================================
   1. Every page renders its injected chrome with zero page errors
   ========================================================================== */

test("every page renders its injected chrome with no page errors", async ({
  page,
}) => {
  const errors = trackPageErrors(page);
  const external: string[] = [];
  page.on("request", (request) => {
    if (!request.url().startsWith("file:")) external.push(request.url());
  });

  for (const entry of registry) {
    await visit(page, entry.slug);

    // The registry parsed from source must be exactly the one the page runs.
    const live = await page.evaluate(() =>
      (
        window as never as { WaypointGuide: { pages: { slug: string }[] } }
      ).WaypointGuide.pages.map((entry) => entry.slug),
    );
    expect(live, `${entry.slug}: running registry differs from source`).toEqual(
      registry.map((page) => page.slug),
    );

    await expect(
      page.locator(".ug-header .ug-brand__name"),
      entry.slug,
    ).toHaveText("Waypoint");
    await expect(page.locator(".ug-nav__item"), entry.slug).toHaveCount(
      registry.length,
    );
    await expect(
      page.locator(".ug-nav__item.is-active"),
      entry.slug,
    ).toHaveAttribute("aria-current", "page");
    await expect(page.locator(".ug-status"), entry.slug).toContainText(
      "Design system",
    );
    await expect(page.locator("h1"), entry.slug).toHaveCount(1);

    // Breadcrumbs appear on every page except the landing page.
    const crumbs = page.locator(".ug-crumbs__list");
    if (entry.slug === "index") {
      await expect(crumbs, entry.slug).toHaveCount(0);
    } else {
      await expect(crumbs, entry.slug).toContainText(entry.category);
    }
  }

  expect(errors, `page errors:\n${errors.join("\n")}`).toEqual([]);
  expect(
    external,
    `the guide must make no external request:\n${external.join("\n")}`,
  ).toEqual([]);
});

/* ==========================================================================
   2. The 404 page renders the chrome and offers a way back
   ========================================================================== */

test("the 404 page renders the chrome and offers a way back", async ({
  page,
}) => {
  const errors = trackPageErrors(page);
  await visit(page, "404");

  await expect(page.locator(".ug-header .ug-brand__name")).toHaveText(
    "Waypoint",
  );
  await expect(page.locator(".ug-nav__item")).toHaveCount(registry.length);
  await expect(page.locator(".ug-status")).toContainText("Design system");
  await expect(page.locator("h1")).toHaveText(
    "That page is not part of this guide",
  );

  const home = page.locator('.ug-404__actions a[href="index.html"]');
  await expect(home).toBeVisible();
  await home.click();
  await expect(page.locator("h1")).toHaveText(
    "Everything Waypoint can do, written for the people who use it",
  );

  expect(errors, `page errors:\n${errors.join("\n")}`).toEqual([]);
});

/* ==========================================================================
   3 & 4. The registry and the file tree must agree, in both directions
   ========================================================================== */

test("every page in the navigation registry exists on disk", () => {
  const onDisk = new Set(authoredSlugs());
  const missing = registry
    .map((entry) => entry.slug)
    .filter((slug) => !onDisk.has(slug));

  expect(
    missing,
    `these slugs are registered in assets/guide.js but have no HTML file:\n${missing.join("\n")}`,
  ).toEqual([]);
});

test("every authored page on disk is registered in the navigation", () => {
  const registered = new Set(registry.map((entry) => entry.slug));
  const unregistered = authoredSlugs().filter(
    (slug) => !registered.has(slug) && !UNREGISTERED_BY_DESIGN.has(slug),
  );

  expect(
    unregistered,
    `these HTML files exist but are missing a registry entry in assets/guide.js:\n${unregistered.join("\n")}`,
  ).toEqual([]);
});

/* ==========================================================================
   5. No internal link or anchor is broken, anywhere
   ========================================================================== */

test("no internal link or anchor is broken", async ({ page }) => {
  const onDisk = new Set(authoredSlugs());
  const broken: string[] = [];

  const allSlugs = [...registry.map((entry) => entry.slug), "404"];

  for (const slug of allSlugs) {
    await visit(page, slug);

    const ids = new Set(
      await page.locator("[id]").evaluateAll((nodes) => nodes.map((n) => n.id)),
    );

    const links = await page
      .locator("a[href]")
      .evaluateAll((nodes) => nodes.map((n) => n.getAttribute("href") ?? ""));

    for (const href of links) {
      if (/^(https?:|mailto:|tel:)/.test(href)) {
        broken.push(`${slug}: external link is not allowed — ${href}`);
        continue;
      }

      const [path, hash] = href.split("#");

      if (!path) {
        // Same-page anchor.
        if (hash && !ids.has(hash))
          broken.push(`${slug}: anchor #${hash} has no matching id`);
        continue;
      }

      if (!path.endsWith(".html")) {
        broken.push(`${slug}: link does not point at a page — ${href}`);
        continue;
      }

      // Resolve relative to the linking page's directory.
      const fromDir = slug.includes("/") ? posix.dirname(slug) : ".";
      const targetSlug = posix
        .normalize(posix.join(fromDir, path))
        .replace(/\.html$/, "")
        .replace(/^\.\//, "");

      if (!onDisk.has(targetSlug)) {
        broken.push(
          `${slug}: link target does not exist — ${href} (resolved to ${targetSlug}.html)`,
        );
      }
    }
  }

  expect(broken, `broken internal links:\n${broken.join("\n")}`).toEqual([]);
});

/* ==========================================================================
   6. The on-page contents links to every section on the page
   ========================================================================== */

test("the on-page contents links to every section on the page", async ({
  page,
}) => {
  for (const entry of registry) {
    await visit(page, entry.slug);

    const sectionIds = await page
      .locator("main section[id]")
      .evaluateAll((nodes) =>
        nodes.filter((n) => n.querySelector(":scope > h2")).map((n) => n.id),
      );

    expect(
      sectionIds.length,
      `${entry.slug} should have at least one identified section`,
    ).toBeGreaterThan(0);

    const tocTargets = await page
      .locator(".ug-toc__link")
      .evaluateAll((nodes) =>
        nodes.map((n) => (n.getAttribute("href") ?? "").replace(/^#/, "")),
      );

    expect(
      tocTargets,
      `${entry.slug}: contents does not match the page's sections`,
    ).toEqual(sectionIds);

    // Every such <h2> also gets an injected anchor link.
    await expect(
      page.locator("main section[id] > h2 .ug-anchor"),
      entry.slug,
    ).toHaveCount(sectionIds.length);
  }
});

/* ==========================================================================
   7. The pager moves forward and back through the reading order
   ========================================================================== */

test("the pager moves forward and back through the reading order", async ({
  page,
}) => {
  // Forward from the first page to the last.
  await visit(page, registry[0].slug);
  await expect(page.locator('a[rel="prev"]')).toHaveCount(0);

  for (let i = 0; i < registry.length - 1; i += 1) {
    const next = page.locator('a[rel="next"]');
    await expect(next, `forward from ${registry[i].slug}`).toContainText(
      registry[i + 1].title,
    );
    await next.click();
    await expect(page.locator(".ug-crumbs__text--current")).toHaveText(
      registry[i + 1].title,
    );
  }

  await expect(page.locator('a[rel="next"]')).toHaveCount(0);

  // And back again to the first page.
  for (let i = registry.length - 1; i > 0; i -= 1) {
    const previous = page.locator('a[rel="prev"]');
    await expect(previous, `back from ${registry[i].slug}`).toContainText(
      registry[i - 1].title,
    );
    await previous.click();
  }

  await expect(page.locator("h1")).toHaveText(
    "Everything Waypoint can do, written for the people who use it",
  );
});

/* ==========================================================================
   8. Search filters the guide and restores focus on Escape
   ========================================================================== */

test("search filters the guide and restores focus on Escape", async ({
  page,
}) => {
  await visit(page, "index");

  // Opened from the header control.
  await page.locator(".ug-searchbtn").click();
  await expect(page.locator(".ug-search__panel")).toBeVisible();
  await expect(page.locator(".ug-search__link")).toHaveCount(registry.length);

  await page.locator(".ug-search__input").fill("volunteer");
  const filtered = await page.locator(".ug-search__link").count();
  expect(filtered).toBeGreaterThan(0);
  expect(filtered).toBeLessThan(registry.length);
  await expect(page.locator(".ug-search__count")).toContainText(
    String(filtered),
  );

  // A search with no match still reports itself rather than showing nothing.
  await page.locator(".ug-search__input").fill("zzzznotathing");
  await expect(page.locator(".ug-search__link")).toHaveCount(0);
  await expect(page.locator(".ug-search__empty")).toBeVisible();

  await page.keyboard.press("Escape");
  await expect(page.locator(".ug-search__panel")).toBeHidden();
  await expect(page.locator(".ug-searchbtn")).toBeFocused();

  // Opened by the keyboard shortcut, focus still returns to a real control.
  await page.keyboard.press("Control+k");
  await expect(page.locator(".ug-search__panel")).toBeVisible();
  await page.keyboard.press("Escape");
  await expect(page.locator(".ug-search__panel")).toBeHidden();
  await expect(page.locator(".ug-searchbtn")).toBeFocused();
});

/* ==========================================================================
   9. Display preferences apply the token contract
   ========================================================================== */

test("display preferences apply the token contract", async ({ page }) => {
  await visit(page, "index");

  const themeToggle = page.locator('.ug-pref[aria-label*="Dark theme"]');
  await expect(themeToggle).toHaveAttribute("aria-pressed", "false");
  await expect(page.locator("html")).toHaveAttribute("data-ug-theme", "light");

  const lightBackground = await page.evaluate(() =>
    getComputedStyle(document.body).getPropertyValue("background-color"),
  );

  await themeToggle.click();
  await expect(themeToggle).toHaveAttribute("aria-pressed", "true");
  await expect(page.locator("html")).toHaveAttribute("data-ug-theme", "dark");

  // The dark theme is built purely by remapping existing --w-* tokens, so
  // --ug-bg must resolve to exactly the value of --w-ink.
  const { background, ugBg, wuInk } = await page.evaluate(() => {
    const root = getComputedStyle(document.documentElement);
    return {
      background: getComputedStyle(document.body).getPropertyValue(
        "background-color",
      ),
      ugBg: root.getPropertyValue("--ug-bg").trim(),
      wuInk: root.getPropertyValue("--w-ink").trim(),
    };
  });

  expect(background).not.toBe(lightBackground);
  expect(ugBg).toBe(wuInk);

  // The preference survives a reload.
  await page.reload();
  await expect(page.locator("html")).toHaveAttribute("data-ug-theme", "dark");
  await expect(
    page.locator('.ug-pref[aria-label*="Dark theme"]'),
  ).toHaveAttribute("aria-pressed", "true");
});

/* ==========================================================================
   10. Reduced motion removes transition durations
   ========================================================================== */

test("reduced motion removes transition durations", async ({ page }) => {
  await visit(page, "index");

  const withMotion = await page.evaluate(
    () =>
      getComputedStyle(document.querySelector(".ug-searchbtn")!)
        .transitionDuration,
  );
  expect(withMotion).not.toBe("0s");

  const motionToggle = page.locator('.ug-pref[aria-label*="Reduced motion"]');
  await expect(motionToggle).toHaveAttribute("aria-pressed", "false");
  await motionToggle.click();
  await expect(motionToggle).toHaveAttribute("aria-pressed", "true");

  await expect(page.locator("html")).toHaveAttribute(
    "data-ug-motion",
    "reduced",
  );
  // The product's own reduced-motion hook is reused, not reinvented.
  await expect(page.locator("html")).toHaveClass(/w-a11y-reduce-motion/);

  const reduced = await page.evaluate(() => {
    const nodes = [
      document.querySelector(".ug-searchbtn"),
      document.querySelector(".ug-card"),
    ];
    return nodes
      .filter(Boolean)
      .map((node) => getComputedStyle(node as Element).transitionDuration);
  });

  for (const duration of reduced) {
    expect(duration.split(",").every((part) => part.trim() === "0s")).toBe(
      true,
    );
  }
});

/* ==========================================================================
   11. Narrow viewports keep the document within the window
   ========================================================================== */

test("narrow viewports keep the document within the window", async ({
  page,
}) => {
  await page.setViewportSize({ width: 320, height: 720 });

  for (const entry of registry) {
    await visit(page, entry.slug);
    const overflow = await page.evaluate(
      () =>
        document.documentElement.scrollWidth -
        document.documentElement.clientWidth,
    );
    expect(
      overflow,
      `${entry.slug} overflows horizontally at 320px by ${overflow}px`,
    ).toBeLessThanOrEqual(1);
  }
});

/* ==========================================================================
   12. The stylesheet contains no hardcoded colour outside the token contract
   ========================================================================== */

test("the stylesheet contains no hardcoded colour outside the token contract", () => {
  const css = readFileSync(join(GUIDE_ROOT, "assets", "guide.css"), "utf8")
    // Strip comments so prose about colour is not mistaken for a value.
    .replace(/\/\*[\s\S]*?\*\//g, "");

  const findings: string[] = [];
  const lines = css.split(/\r?\n/);

  // Hex colours, and any rgb()/hsl()/color() function call. `color-mix` is
  // allowed because it can only combine values that are already tokens.
  const patterns: Array<[RegExp, string]> = [
    [/#[0-9a-fA-F]{3,8}\b/, "hex colour"],
    [/\brgba?\s*\(/, "rgb() colour"],
    [/\bhsla?\s*\(/, "hsl() colour"],
    [/\b(?:lab|lch|oklab|oklch)\s*\(/, "colour function"],
  ];

  // Named CSS colours that would slip past the patterns above.
  const namedColours =
    /\b(?:aqua|black|blue|fuchsia|gray|grey|green|lime|maroon|navy|olive|orange|purple|red|silver|teal|white|yellow|rebeccapurple)\b\s*(?:;|\)|,|$)/;

  lines.forEach((line, index) => {
    const trimmed = line.trim();
    if (!trimmed) return;

    // Only property declarations can carry a colour value.
    if (!trimmed.includes(":")) return;

    for (const [pattern, label] of patterns) {
      if (pattern.test(trimmed)) {
        findings.push(`assets/guide.css:${index + 1}  ${label} — ${trimmed}`);
      }
    }

    const property = trimmed.split(":")[0].trim().toLowerCase();
    const isColourProperty =
      /color|background|border|outline|fill|stroke|shadow/.test(property);

    // Strip custom-property names before looking for named colours, so a
    // reference to a token such as `var(--w-white)` is not mistaken for the
    // literal keyword `white`. A genuine literal (`--ug-x: white;`) survives
    // this and is still reported.
    const withoutTokenNames = trimmed.replace(/--[a-zA-Z0-9-]+/g, "");

    if (isColourProperty && namedColours.test(withoutTokenNames)) {
      findings.push(`assets/guide.css:${index + 1}  named colour — ${trimmed}`);
    }
  });

  expect(
    findings,
    `every colour must resolve through the --w-* token contract:\n${findings.join("\n")}`,
  ).toEqual([]);
});
