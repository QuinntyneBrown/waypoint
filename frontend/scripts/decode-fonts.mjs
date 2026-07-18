#!/usr/bin/env node
// One-time / reproducible: decode the six base64 `data:font/woff2` URIs in
// docs/mocks/assets/css/_fonts.css into real .woff2 binaries under an app's
// public/fonts/ directory. Keeps ~178 KB of base64 out of the JS/CSS bundle
// (frontend-implementation-plan.md Risks #1). Idempotent.
//
// Usage: node scripts/decode-fonts.mjs [targetPublicDir]
//   default target: projects/portal/public/fonts
import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const HERE = dirname(fileURLToPath(import.meta.url));
const FRONTEND = join(HERE, '..');
const REPO = join(FRONTEND, '..');
const SRC = join(REPO, 'docs', 'mocks', 'assets', 'css', '_fonts.css');
const target = process.argv[2] ?? join(FRONTEND, 'projects', 'portal', 'public', 'fonts');

const slug = (family) =>
  family
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');

const css = readFileSync(SRC, 'utf8');
const re =
  /@font-face\{font-family:'([^']+)';font-style:(\w+);font-weight:(\d+);[^}]*?src:url\(data:font\/woff2;base64,([A-Za-z0-9+/=]+)\)/g;

mkdirSync(target, { recursive: true });
let m;
let count = 0;
const faces = [];
while ((m = re.exec(css)) !== null) {
  const [, family, style, weight, b64] = m;
  const file = `${slug(family)}-${weight}.woff2`;
  writeFileSync(join(target, file), Buffer.from(b64, 'base64'));
  faces.push({ family, style, weight, file });
  count++;
}

if (count === 0) {
  console.error('No @font-face rules matched — check _fonts.css format.');
  process.exit(1);
}
console.log(`Decoded ${count} fonts into ${target}`);
for (const f of faces) console.log(`  ${f.file}  (${f.family} ${f.weight})`);
