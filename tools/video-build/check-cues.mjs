// Checks a video folder's text files before synthesis (no audio needed).
//   node tools/video-build/check-cues.mjs docs/videos/NNN-topic
// - every slide except the first has a data-cue that appears verbatim and once in script.md
// - cues are in narration order
// - every <img src> in slides.html exists
import { existsSync, readFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

const dir = resolve(process.argv[2] ?? '.');
const script = readFileSync(join(dir, 'script.md'), 'utf8').replace(/\r\n/g, '\n');
const html = readFileSync(join(dir, 'slides.html'), 'utf8');
const problems = [];
let last = -1;
const sections = [...html.matchAll(/<section\b([^>]*)>/g)];
sections.forEach((m, i) => {
  const cue = m[1].match(/data-cue="([^"]*)"/)?.[1]?.replace(/&amp;/g, '&').replace(/&quot;/g, '"');
  if (!cue) {
    if (i > 0) problems.push(`slide ${i + 1} has no data-cue`);
    return;
  }
  const first = script.indexOf(cue);
  if (first < 0) return problems.push(`slide ${i + 1}: cue not found verbatim: "${cue}"`);
  if (script.indexOf(cue, first + 1) >= 0) problems.push(`slide ${i + 1}: cue is not unique: "${cue}"`);
  if (first <= last) problems.push(`slide ${i + 1}: cue is out of narration order: "${cue}"`);
  last = first;
});
for (const m of html.matchAll(/<img\b[^>]*\bsrc="([^"]+)"/g)) {
  if (!/^https?:/.test(m[1]) && !existsSync(join(dir, m[1]))) problems.push(`missing image ${m[1]}`);
}
if (problems.length) {
  console.error(problems.join('\n'));
  process.exit(1);
}
console.log(`ok: ${sections.length} slides`);
