#!/usr/bin/env node
// Requirement traceability gate for Word Up.
// - Collects every in-scope L2 id from docs/specs/**/L2.md.
// - Collects every L2 cited by tests: backend `[Property("Req","L2-…")]` and
//   frontend/e2e `// Traces to: L2-…` headers.
// - HARD FAILS if any test cites an L2 that is not an in-scope requirement
//   (out-of-scope / retired / typo). Reports forward coverage (in-scope L2 -> tests).
// - Validates explicit `AC1`, `AC1/AC3`, and `AC1..AC3` citations against the
//   numbered acceptance criteria in the owning L2 section and reports AC coverage.
// - With --strict, also fails if any in-scope L2 has zero covering tests
//   (use once a subsystem/build is meant to be complete).
// - With --strict-ac, also fails if any individual acceptance criterion has no
//   explicit citation. This is opt-in while legacy tests gain AC annotations.
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = join(dirname(fileURLToPath(import.meta.url)), "..");
const strict = process.argv.includes("--strict");
const strictAc = process.argv.includes("--strict-ac");

function walk(dir, out = []) {
  let entries = [];
  try {
    entries = readdirSync(dir);
  } catch {
    return out;
  }
  for (const e of entries) {
    const p = join(dir, e);
    const s = statSync(p);
    if (s.isDirectory()) {
      if (!["node_modules", "bin", "obj", ".git", "dist"].includes(e))
        walk(p, out);
    } else out.push(p);
  }
  return out;
}

// 1. In-scope L2 ids
const specFiles = walk(join(ROOT, "docs", "specs")).filter((f) =>
  f.endsWith("L2.md"),
);
const inScope = new Set();
const acceptanceCriteria = new Map(); // L2 -> numbered AC count
for (const f of specFiles) {
  const text = readFileSync(f, "utf8");
  const headings = [...text.matchAll(/^##\s+(L2-[A-Z]+-\d+).*$/gm)];
  for (let index = 0; index < headings.length; index += 1) {
    const heading = headings[index];
    const id = heading[1];
    const sectionEnd = headings[index + 1]?.index ?? text.length;
    const section = text.slice(heading.index, sectionEnd);
    const criteria = section.split("**Acceptance Criteria:**")[1] ?? "";
    const numbers = [...criteria.matchAll(/^\s*(\d+)\.\s+/gm)].map((match) =>
      Number(match[1]),
    );
    inScope.add(id);
    acceptanceCriteria.set(id, numbers.length ? Math.max(...numbers) : 0);
  }
}

// 2. Test citations
const L2_REF = /L2-[A-Z]+-\d+/g;
const testFiles = [
  ...walk(join(ROOT, "backend", "tests")).filter((f) => f.endsWith(".cs")),
  ...walk(join(ROOT, "frontend", "e2e")).filter(
    (f) => f.endsWith(".spec.ts") || f.endsWith(".ts"),
  ),
  ...walk(join(ROOT, "scripts", "performance")).filter((f) =>
    f.endsWith(".mjs"),
  ),
];
const cited = new Map(); // L2 -> Set(file)
const citedAc = new Map(); // L2#AC -> Set(file)

function acceptanceCriterionNumbers(text) {
  const numbers = new Set();
  for (const match of text.matchAll(
    /\bAC\s*(\d+)\s*(?:\.\.|-|–|—)\s*(?:AC\s*)?(\d+)/gi,
  )) {
    const first = Number(match[1]);
    const last = Number(match[2]);
    const lower = Math.min(first, last);
    const upper = Math.max(first, last);
    for (let number = lower; number <= upper; number += 1) numbers.add(number);
  }
  for (const match of text.matchAll(/\bAC\s*(\d+)/gi))
    numbers.add(Number(match[1]));
  return numbers;
}

for (const f of testFiles) {
  const txt = readFileSync(f, "utf8");
  const relativeFile = f.replace(ROOT + "\\", "").replace(ROOT + "/", "");
  // only count Property("Req","L2-…") or "Traces to: L2-…"
  const refs = new Set();
  for (const m of txt.matchAll(/Property\(\s*"Req"\s*,\s*"(L2-[A-Z]+-\d+)"/g))
    refs.add(m[1]);
  for (const trace of txt.matchAll(/Traces to:\s*([^\n]+)/g)) {
    const traceText = trace[1];
    const requirementMatches = [...traceText.matchAll(L2_REF)];
    for (let index = 0; index < requirementMatches.length; index += 1) {
      const requirement = requirementMatches[index];
      const id = requirement[0];
      refs.add(id);
      const detailStart = (requirement.index ?? 0) + id.length;
      const detailEnd =
        requirementMatches[index + 1]?.index ?? traceText.length;
      for (const criterion of acceptanceCriterionNumbers(
        traceText.slice(detailStart, detailEnd),
      )) {
        const key = `${id}#AC${criterion}`;
        if (!citedAc.has(key)) citedAc.set(key, new Set());
        citedAc.get(key).add(relativeFile);
      }
    }
  }
  for (const r of refs) {
    if (!cited.has(r)) cited.set(r, new Set());
    cited.get(r).add(relativeFile);
  }
}

// 3. Reconcile
const invalid = [...cited.keys()].filter((id) => !inScope.has(id)).sort();
const covered = [...inScope].filter((id) => cited.has(id)).sort();
const uncovered = [...inScope].filter((id) => !cited.has(id)).sort();
const totalAcceptanceCriteria = [...acceptanceCriteria.values()].reduce(
  (sum, count) => sum + count,
  0,
);
const invalidAc = [...citedAc.entries()]
  .filter(([key]) => {
    const [id, ac] = key.split("#AC");
    return (
      !acceptanceCriteria.has(id) ||
      Number(ac) < 1 ||
      Number(ac) > acceptanceCriteria.get(id)
    );
  })
  .sort(([left], [right]) => left.localeCompare(right));
const uncoveredAc = [];
for (const [id, count] of acceptanceCriteria) {
  for (let number = 1; number <= count; number += 1) {
    const key = `${id}#AC${number}`;
    if (!citedAc.has(key)) uncoveredAc.push(key);
  }
}

console.log(
  `Traceability: ${inScope.size} in-scope L2 requirements, ${covered.length} covered by tests, ` +
    `${uncovered.length} not yet covered; ${citedAc.size - invalidAc.length}/${totalAcceptanceCriteria} acceptance criteria explicitly covered.`,
);
if (invalid.length) {
  console.error(
    `\n✗ ${invalid.length} test(s) cite an L2 that is NOT an in-scope requirement (out-of-scope/retired/typo):`,
  );
  for (const id of invalid)
    console.error(`   ${id}  <- ${[...cited.get(id)].join(", ")}`);
}
if (invalidAc.length) {
  console.error(
    `\n✗ ${invalidAc.length} test citation(s) reference an acceptance criterion that does not exist:`,
  );
  for (const [key, files] of invalidAc)
    console.error(`   ${key}  <- ${[...files].join(", ")}`);
}
if (strict && uncovered.length) {
  console.error(
    `\n✗ ${uncovered.length} in-scope L2 requirement(s) have NO covering test:`,
  );
  for (const id of uncovered) console.error(`   ${id}`);
}
if (strictAc && uncoveredAc.length) {
  console.error(
    `\n✗ ${uncoveredAc.length} acceptance criterion/criteria have NO explicit test citation:`,
  );
  for (const key of uncoveredAc) console.error(`   ${key}`);
}

const failed =
  invalid.length > 0 ||
  invalidAc.length > 0 ||
  (strict && uncovered.length > 0) ||
  (strictAc && uncoveredAc.length > 0);
console.log(
  failed
    ? "\nTRACEABILITY GATE: FAIL"
    : "\nTRACEABILITY GATE: PASS (no invalid references)",
);
process.exit(failed ? 1 : 0);
