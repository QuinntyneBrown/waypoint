#!/usr/bin/env node
// Builds <folder>/<name>.mp4 from slides.html, the narration MP3 and its timing manifest.
//
//   node tools/video-build/build-video.mjs <folder> [--check] [--slides-only] [--burn-captions]
//
//   --check        resolve every data-cue and print the schedule
//   --slides-only  also render slide PNGs to <folder>/.cache/ (no encode)
//   --burn-captions  burn captions into the picture; default is a selectable subtitle track
// Env: EDGE_PATH (Microsoft Edge or Chrome), FFMPEG_PATH (ffmpeg).
// Run tools/video-audio/generate-audio.mjs first: it writes the MP3 and .cache/timing.json.
import { spawnSync } from 'node:child_process';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { basename, join, resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const FFMPEG = process.env.FFMPEG_PATH || 'ffmpeg';
const EDGE_CANDIDATES = [
  process.env.EDGE_PATH,
  'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe',
  'C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe',
  'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe',
].filter(Boolean);

const fail = (message) => {
  console.error(`error: ${message}`);
  process.exit(1);
};

const args = process.argv.slice(2);
const folderArg = args.find((a) => !a.startsWith('--'));
if (!folderArg) fail('usage: build-video.mjs <folder> [--check] [--slides-only] [--burn-captions]');
const folder = resolve(folderArg);
const name = basename(folder);
const cache = join(folder, '.cache');
const slidesFile = join(folder, 'slides.html');
const timingFile = join(cache, 'timing.json');
if (!existsSync(timingFile)) fail('missing .cache/timing.json: run tools/video-audio/generate-audio.mjs first');
const manifest = JSON.parse(readFileSync(timingFile, 'utf8'));

// ---------- slides and cues ----------

const html = readFileSync(slidesFile, 'utf8');
const slides = [...html.matchAll(/<section\b([^>]*)>/g)].map((m, i) => ({
  n: i + 1,
  id: /\bid="([^"]*)"/.exec(m[1])?.[1] ?? `slide-${i + 1}`,
  cue: /\bdata-cue="([^"]*)"/.exec(m[1])?.[1]?.replace(/&amp;/g, '&'),
}));
if (!slides.length) fail('slides.html has no <section>');

const paragraphs = manifest.sections.flatMap((s) => s.paragraphs.filter((p) => p.text));

function cueTime(cue) {
  const section = manifest.sections.find((s) => s.title === cue);
  if (section) return section.start;
  const hits = paragraphs.filter((p) => p.text.includes(cue));
  if (hits.length !== 1) fail(`cue "${cue}" matched ${hits.length} paragraphs (needs exactly 1)`);
  const p = hits[0];
  const before = p.text.slice(0, p.text.indexOf(cue)).split(/\s+/).filter(Boolean).length;
  return p.start + ((p.end - p.start) * before) / p.words;
}

slides.forEach((s, i) => {
  if (i === 0) s.start = 0;
  else if (!s.cue) fail(`slide ${s.n} (${s.id}) needs a data-cue`);
  else s.start = cueTime(s.cue);
});
slides.forEach((s, i) => {
  s.end = i + 1 < slides.length ? slides[i + 1].start : manifest.total;
  s.duration = s.end - s.start;
  if (s.duration <= 0) fail(`slide ${s.n} (${s.id}) cue is out of narration order`);
});

const mmss = (t) => `${String(Math.floor(t / 60)).padStart(2, '0')}:${String(Math.floor(t % 60)).padStart(2, '0')}`;
for (const s of slides) {
  const warn = s.duration < 4 ? '  WARNING: shorter than 4s' : s.duration > 90 ? '  WARNING: longer than 90s' : '';
  console.log(`${String(s.n).padStart(2)}  ${s.id.padEnd(12)} ${mmss(s.start)}  ${s.duration.toFixed(1).padStart(5)}s${warn}`);
}
if (args.includes('--check')) process.exit(0);

// ---------- slide images ----------

mkdirSync(cache, { recursive: true });
const edge = EDGE_CANDIDATES.find((p) => existsSync(p));
if (!edge) fail('no Edge or Chrome found: set EDGE_PATH');
const profile = join(cache, 'browser-profile');

for (const s of slides) {
  const png = join(cache, `slide-${String(s.n).padStart(2, '0')}.png`);
  spawnSync(edge, [
    '--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
    '--window-size=1920,1080', `--user-data-dir=${profile}`, '--virtual-time-budget=3000',
    `--screenshot=${png}`, `${pathToFileURL(slidesFile).href}?slide=${s.n}`,
  ], { stdio: 'ignore' });
  if (!existsSync(png)) fail(`browser did not write ${png}`);
}
console.log(`rendered ${slides.length} slide images to .cache/`);
if (args.includes('--slides-only')) process.exit(0);

// ---------- captions ----------

const srtTime = (t) => {
  const ms = Math.round(t * 1000);
  const p = (n, w) => String(n).padStart(w, '0');
  return `${p(Math.floor(ms / 3600000), 2)}:${p(Math.floor(ms / 60000) % 60, 2)}:${p(Math.floor(ms / 1000) % 60, 2)},${p(ms % 1000, 3)}`;
};

function wrap(text, width = 36) {
  const lines = [];
  let line = '';
  for (const word of text.split(' ')) {
    if (line && (line + ' ' + word).length > width) {
      lines.push(line);
      line = word;
    } else line = line ? `${line} ${word}` : word;
  }
  if (line) lines.push(line);
  return lines.join('\n');
}

const cues = [];
for (const p of paragraphs) {
  const words = p.text.split(/\s+/).filter(Boolean);
  const chunks = [];
  let current = [];
  for (const w of words) {
    current.push(w);
    const sentenceEnd = /[.?!]$/.test(w);
    const softEnd = /,$/.test(w) && current.length >= 6;
    if (sentenceEnd || softEnd || current.length >= 10) {
      chunks.push(current);
      current = [];
    }
  }
  if (current.length) chunks.push(current);
  let t = p.start;
  const perWord = (p.end - p.start) / words.length;
  for (const c of chunks) {
    const end = t + perWord * c.length;
    cues.push({ start: t, end, text: wrap(c.join(' ')) });
    t = end;
  }
}
writeFileSync(
  join(cache, 'captions.srt'),
  cues.map((c, i) => `${i + 1}\n${srtTime(c.start)} --> ${srtTime(c.end)}\n${c.text}\n`).join('\n'),
);

// ---------- encode ----------

const concat = slides
  .map((s) => `file 'slide-${String(s.n).padStart(2, '0')}.png'\nduration ${s.duration.toFixed(3)}`)
  .join('\n');
const lastPng = `slide-${String(slides.length).padStart(2, '0')}.png`;
writeFileSync(join(cache, 'frames.txt'), `${concat}\nfile '${lastPng}'\n`);

const style =
  "FontName=Segoe UI,Bold=1,FontSize=15,PrimaryColour=&H00FFFFFF,OutlineColour=&H001D2126," +
  'BorderStyle=3,Outline=6,Shadow=0,Alignment=2,MarginV=36';
const burn = args.includes('--burn-captions');
const result = spawnSync(
  FFMPEG,
  [
    '-y', '-loglevel', 'error', '-f', 'concat', '-safe', '0', '-i', 'frames.txt', '-i', `../${name}.mp3`,
    ...(burn ? [] : ['-i', 'captions.srt']),
    '-vf', burn ? `subtitles=captions.srt:force_style='${style}',format=yuv420p` : 'format=yuv420p',
    '-map', '0:v', '-map', '1:a', ...(burn ? [] : ['-map', '2:s', '-c:s', 'mov_text', '-metadata:s:s:0', 'language=eng']),
    '-r', '30', '-c:v', 'libx264', '-preset', 'medium', '-crf', '20',
    '-c:a', 'aac', '-b:a', '160k', '-t', manifest.total.toFixed(3), '-movflags', '+faststart', `../${name}.mp4`,
  ],
  { cwd: cache, encoding: 'utf8' },
);
if (result.status !== 0) fail(`ffmpeg failed: ${(result.stderr || result.error?.message || '').slice(-800)}`);
console.log(`wrote ${name}.mp4`);
