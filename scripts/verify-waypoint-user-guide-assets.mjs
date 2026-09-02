#!/usr/bin/env node
// Vendored asset gate for the Waypoint user guide.
// - The guide at `user-guides/` deploys on its own, independently of the
//   applications, so it cannot reach a sibling directory at runtime. It carries
//   byte-identical copies of the design-system stylesheets in
//   `user-guides/assets/vendor/`.
// - This gate sha256-compares every vendored copy against its source in
//   `frontend/projects/w-ui/src/styles/` and FAILS if any pair differs, so the
//   guide can never silently drift away from the product's own styling.
// - With --fix, re-copies each drifted source over its vendored copy instead of
//   failing. That is the only supported way to update a vendored file.
//
// Note: `fonts.css` is deliberately NOT vendored. It declares @font-face rules
// with absolute `/fonts/*.woff2` URLs that cannot resolve when a page is opened
// over file://, and the guide is required to make no external font request. The
// `--w-font-*` tokens already carry complete system-font fallback stacks.
import { createHash } from "node:crypto";
import { copyFileSync, existsSync, readFileSync } from "node:fs";
import { dirname, join, relative } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = join(dirname(fileURLToPath(import.meta.url)), "..");
const fix = process.argv.includes("--fix");

const SOURCE_DIR = join(ROOT, "frontend", "projects", "w-ui", "src", "styles");
const VENDOR_DIR = join(ROOT, "user-guides", "assets", "vendor");

/** Every stylesheet the guide vendors, by file name. */
const VENDORED = ["core.css", "a11y.css"];

function sha256(path) {
  return createHash("sha256").update(readFileSync(path)).digest("hex");
}

function rel(path) {
  return relative(ROOT, path).split("\\").join("/");
}

let failed = 0;
let fixed = 0;
let matched = 0;

for (const name of VENDORED) {
  const source = join(SOURCE_DIR, name);
  const vendored = join(VENDOR_DIR, name);

  if (!existsSync(source)) {
    console.error(
      `MISSING SOURCE  ${name}\n` +
        `  Expected the design-system stylesheet at ${rel(source)}.\n` +
        `  If it moved, update VENDORED/SOURCE_DIR in ${rel(fileURLToPath(import.meta.url))}.`,
    );
    failed += 1;
    continue;
  }

  if (!existsSync(vendored)) {
    if (fix) {
      copyFileSync(source, vendored);
      console.log(`COPIED          ${name}  (was missing)`);
      fixed += 1;
    } else {
      console.error(
        `MISSING VENDOR  ${name}\n` +
          `  Expected a vendored copy at ${rel(vendored)}.\n` +
          `  Fix: node ${rel(fileURLToPath(import.meta.url))} --fix`,
      );
      failed += 1;
    }
    continue;
  }

  const sourceHash = sha256(source);
  const vendoredHash = sha256(vendored);

  if (sourceHash === vendoredHash) {
    matched += 1;
    continue;
  }

  if (fix) {
    copyFileSync(source, vendored);
    console.log(`UPDATED         ${name}  ${vendoredHash.slice(0, 12)} -> ${sourceHash.slice(0, 12)}`);
    fixed += 1;
    continue;
  }

  console.error(
    `DRIFT           ${name}\n` +
      `  source   ${rel(source)}\n` +
      `           sha256 ${sourceHash}\n` +
      `  vendored ${rel(vendored)}\n` +
      `           sha256 ${vendoredHash}\n` +
      `  The vendored copy must be byte-identical to the design system.\n` +
      `  Fix: node ${rel(fileURLToPath(import.meta.url))} --fix`,
  );
  failed += 1;
}

if (fix) {
  console.log(
    `\n${VENDORED.length} vendored stylesheet(s): ${fixed} updated, ${matched} already identical.`,
  );
} else {
  console.log(`\n${VENDORED.length} vendored stylesheet(s): ${matched} identical, ${failed} drifted.`);
}

console.log(`USER GUIDE ASSET GATE: ${failed ? "FAIL" : "PASS"}`);
process.exit(failed ? 1 : 0);
