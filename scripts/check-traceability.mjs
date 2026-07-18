#!/usr/bin/env node
// Requirement traceability gate for Word Up.
// - Collects every in-scope L2 id from docs/specs/**/L2.md.
// - Collects every L2 cited by tests: backend `[Property("Req","L2-…")]` and
//   frontend/e2e `// Traces to: L2-…` headers.
// - HARD FAILS if any test cites an L2 that is not an in-scope requirement
//   (out-of-scope / retired / typo). Reports forward coverage (in-scope L2 -> tests).
// - With --strict, also fails if any in-scope L2 has zero covering tests
//   (use once a subsystem/build is meant to be complete).
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const strict = process.argv.includes('--strict');

function walk(dir, out = []) {
  let entries = [];
  try { entries = readdirSync(dir); } catch { return out; }
  for (const e of entries) {
    const p = join(dir, e);
    const s = statSync(p);
    if (s.isDirectory()) { if (!['node_modules', 'bin', 'obj', '.git', 'dist'].includes(e)) walk(p, out); }
    else out.push(p);
  }
  return out;
}

// 1. In-scope L2 ids
const specFiles = walk(join(ROOT, 'docs', 'specs')).filter(f => f.endsWith('L2.md'));
const inScope = new Set();
for (const f of specFiles) {
  for (const m of readFileSync(f, 'utf8').matchAll(/^##\s+(L2-[A-Z]+-\d+)/gm)) inScope.add(m[1]);
}

// 2. Test citations
const L2_REF = /L2-[A-Z]+-\d+/g;
const testFiles = [
  ...walk(join(ROOT, 'backend', 'tests')).filter(f => f.endsWith('.cs')),
  ...walk(join(ROOT, 'frontend', 'e2e')).filter(f => f.endsWith('.spec.ts') || f.endsWith('.ts')),
];
const cited = new Map(); // L2 -> Set(file)
for (const f of testFiles) {
  const txt = readFileSync(f, 'utf8');
  // only count Property("Req","L2-…") or "Traces to: L2-…"
  const refs = new Set();
  for (const m of txt.matchAll(/Property\(\s*"Req"\s*,\s*"(L2-[A-Z]+-\d+)"/g)) refs.add(m[1]);
  for (const m of txt.matchAll(/Traces to:\s*([^\n]+)/g)) for (const r of m[1].matchAll(L2_REF)) refs.add(r[0]);
  for (const r of refs) { if (!cited.has(r)) cited.set(r, new Set()); cited.get(r).add(f.replace(ROOT + '\\', '').replace(ROOT + '/', '')); }
}

// 3. Reconcile
const invalid = [...cited.keys()].filter(id => !inScope.has(id)).sort();
const covered = [...inScope].filter(id => cited.has(id)).sort();
const uncovered = [...inScope].filter(id => !cited.has(id)).sort();

console.log(`Traceability: ${inScope.size} in-scope L2 requirements, ${covered.length} covered by tests, ${uncovered.length} not yet covered.`);
if (invalid.length) {
  console.error(`\n✗ ${invalid.length} test(s) cite an L2 that is NOT an in-scope requirement (out-of-scope/retired/typo):`);
  for (const id of invalid) console.error(`   ${id}  <- ${[...cited.get(id)].join(', ')}`);
}
if (strict && uncovered.length) {
  console.error(`\n✗ ${uncovered.length} in-scope L2 requirement(s) have NO covering test:`);
  for (const id of uncovered) console.error(`   ${id}`);
}

const failed = invalid.length > 0 || (strict && uncovered.length > 0);
console.log(failed ? '\nTRACEABILITY GATE: FAIL' : '\nTRACEABILITY GATE: PASS (no invalid references)');
process.exit(failed ? 1 : 0);
