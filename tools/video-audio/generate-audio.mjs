#!/usr/bin/env node
// Narration generator for docs/videos/NN-topic/script.md using free edge-tts.
//
//   node tools/video-audio/generate-audio.mjs <folder> [--dry-run] [--engine edge|piper|kokoro|sapi]
//   node tools/video-audio/generate-audio.mjs --say "text" --out file.wav
//
// --engine edge (default): free edge-tts neural voices, needs internet.
// --engine piper: offline neural voices from Hugging Face (free, no internet
//   needed once downloaded). One-time setup, in any Python 3.12 or newer:
//     python -m venv .cache/piper-venv
//     .cache/piper-venv/Scripts/python -m pip install piper-tts
//     download en_US-lessac-high.onnx and en_US-lessac-high.onnx.json from
//     https://huggingface.co/rhasspy/piper-voices/tree/main/en/en_US/lessac/high
//     into .cache/piper/
//   then set PIPER_PYTHON=.cache/piper-venv/Scripts/python.exe. On Windows ARM
//   use Python 3.14; the 3.12 piper-tts wheel lacks its phoneme library.
// --engine kokoro: offline Kokoro-82M voices (Apache-2.0), warmer and more
//   natural than Piper. Free, no API key, no internet once downloaded. One-time
//   setup, with Python 3.10 to 3.12:
//     python -m pip install kokoro-onnx soundfile
//     download onnx/model.onnx as model.onnx, and the voices you want from
//     voices/ (for example af_heart.bin), from
//     https://huggingface.co/onnx-community/Kokoro-82M-v1.0-ONNX
//     into .cache/kokoro/ and .cache/kokoro/voices/
//   then set KOKORO_PYTHON to that Python if it is not the default.
// --engine sapi: offline Windows voices; lowest quality, no setup.
// Env: PYTHON (python), FFMPEG_PATH (ffmpeg), FFPROBE_PATH (ffprobe),
//      EDGE_VOICE (en-US-AndrewMultilingualNeural),
//      EDGE_VOICE_2 (en-US-AvaMultilingualNeural), EDGE_RATE (-10%),
//      SAPI_VOICE (Microsoft Zira Desktop), SAPI_VOICE_2 (Microsoft David
//      Desktop), SAPI_RATE (-2), PIPER_PYTHON, PIPER_MODEL
//      (.cache/piper/en_US-lessac-high.onnx), PIPER_LENGTH_SCALE (1.3),
//      KOKORO_PYTHON (python), KOKORO_DIR (.cache/kokoro), KOKORO_VOICE
//      (af_heart), KOKORO_SPEED (0.8, below 1 is slower).
// Output: <folder>/<folder-name>.mp3 and <folder>/.cache/timing.json.
import { spawnSync } from 'node:child_process';
import { createHash } from 'node:crypto';
import { existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { basename, join, resolve } from 'node:path';

const PYTHON = process.env.PYTHON || 'python';
const FFMPEG = process.env.FFMPEG_PATH || 'ffmpeg';
const FFPROBE = process.env.FFPROBE_PATH || 'ffprobe';
const VOICE = process.env.EDGE_VOICE || 'en-US-AndrewMultilingualNeural';
const VOICE_2 = process.env.EDGE_VOICE_2 || 'en-US-AvaMultilingualNeural';
const RATE = process.env.EDGE_RATE || '-10%';
const WORDS_PER_MINUTE = 150;
const args = process.argv.slice(2);
// "edge" (default, neural voices, needs internet) or "sapi" (offline Windows voices).
const ENGINE = args.includes('--engine') ? args[args.indexOf('--engine') + 1] : 'edge';
if (!['edge', 'sapi', 'piper', 'kokoro'].includes(ENGINE)) fail(`unknown engine "${ENGINE}" (use edge, sapi, piper or kokoro)`);
// Piper: offline neural voices from huggingface.co/rhasspy/piper-voices.
const REPO_ROOT = resolve(import.meta.dirname, '..', '..');
const PIPER_PYTHON = process.env.PIPER_PYTHON || PYTHON; // a Python with piper-tts installed
const PIPER_MODEL = process.env.PIPER_MODEL || join(REPO_ROOT, '.cache', 'piper', 'en_US-lessac-high.onnx');
const PIPER_LENGTH_SCALE = process.env.PIPER_LENGTH_SCALE || '1.3'; // above 1 is slower
// Kokoro: offline voices from huggingface.co/onnx-community/Kokoro-82M-v1.0-ONNX.
const KOKORO_PYTHON = process.env.KOKORO_PYTHON || PYTHON; // a Python with kokoro-onnx and soundfile
const KOKORO_DIR = process.env.KOKORO_DIR || join(REPO_ROOT, '.cache', 'kokoro');
const KOKORO_VOICE = process.env.KOKORO_VOICE || 'af_heart';
const KOKORO_SPEED = process.env.KOKORO_SPEED || '0.8'; // below 1 is slower
const SAPI_VOICE = process.env.SAPI_VOICE || 'Microsoft Zira Desktop';
const SAPI_VOICE_2 = process.env.SAPI_VOICE_2 || 'Microsoft David Desktop';
const SAPI_RATE = process.env.SAPI_RATE || '-2'; // -10 slowest .. 10 fastest

function fail(message) {
  console.error(`error: ${message}`);
  process.exit(1);
}

function run(cmd, args) {
  const r = spawnSync(cmd, args, { encoding: 'utf8' });
  if (r.error) fail(`could not run ${cmd}: ${r.error.message}`);
  if (r.status !== 0) fail(`${cmd} failed (${r.status}): ${(r.stderr || '').trim().slice(-600)}`);
  return r.stdout;
}

const countWords = (text) => text.split(/\s+/).filter(Boolean).length;

// ---------- script parsing ----------

export function parseScript(text) {
  const lines = text.replace(/\r\n/g, '\n').split('\n');
  const title = lines[0].match(/^# (\d{2,3}) · (.+)$/);
  if (!title) fail('line 1 must be "# NN · Title"');
  const sections = [];
  let section = null;
  let buffer = [];
  let bufferLine = 0;

  const flush = () => {
    if (!buffer.length) return;
    if (!section) fail(`line ${bufferLine}: narration before the first "## Section"`);
    let body = buffer.join(' ').trim();
    let speaker = 1;
    const named = body.match(/^\*\*[^*]+:\*\*\s*(.*)$/);
    if (named) {
      speaker = 2;
      body = named[1];
    }
    body = body.replace(/^- /, '');
    section.items.push({ type: 'speech', speaker, text: body, words: countWords(body) });
    buffer = [];
  };

  for (let i = 1; i < lines.length; i++) {
    const line = lines[i];
    const n = i + 1;
    if (/^\s*\|/.test(line)) fail(`line ${n}: tables are not allowed`);
    if (/^\s*```/.test(line)) fail(`line ${n}: fenced code is not allowed`);
    if (/<[a-z/][^>]*>/i.test(line)) fail(`line ${n}: HTML is not allowed`);
    if (/\]\(/.test(line)) fail(`line ${n}: links are not allowed`);
    const heading = line.match(/^## (.+)$/);
    if (heading) {
      flush();
      section = { title: heading[1].trim(), items: [] };
      sections.push(section);
      continue;
    }
    if (!line.trim()) {
      flush();
      continue;
    }
    const pause = line.match(/^\[pause (\d+(?:\.\d+)?)s\]$/);
    if (pause) {
      flush();
      if (!section) fail(`line ${n}: pause before the first section`);
      section.items.push({ type: 'pause', seconds: Number(pause[1]) });
      continue;
    }
    if (/^- /.test(line)) flush();
    if (!buffer.length) bufferLine = n;
    buffer.push(line.trim());
  }
  flush();
  if (!sections.length) fail('script has no "## Section"');
  return { number: title[1], title: title[2], sections };
}

function applyLexicon(text, lexicon) {
  let out = text.replace(/`/g, '');
  for (const [from, to] of Object.entries(lexicon)) {
    const escaped = from.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    out = out.replace(new RegExp(`\\b${escaped}\\b`, 'g'), to);
  }
  return out;
}

// ---------- synthesis ----------

// Offline fallback: Windows built-in voices through System.Speech. Used when the
// edge-tts service is unreachable (for example it answers 403 for this network).
// Text travels in environment variables so no quoting is needed.
function synthesizeSapi(text, voice, outWav) {
  const script =
    'Add-Type -AssemblyName System.Speech;' +
    '$s = New-Object System.Speech.Synthesis.SpeechSynthesizer;' +
    '$s.SelectVoice($env:SAPI_VOICE);' +
    '$s.Rate = [int]$env:SAPI_RATE;' +
    '$s.SetOutputToWaveFile($env:SAPI_OUT);' +
    '$s.Speak($env:SAPI_TEXT);' +
    '$s.Dispose()';
  const r = spawnSync('powershell.exe', ['-NoProfile', '-NonInteractive', '-Command', script], {
    encoding: 'utf8',
    env: { ...process.env, SAPI_TEXT: text, SAPI_VOICE: voice, SAPI_RATE: SAPI_RATE, SAPI_OUT: outWav },
  });
  if (r.status !== 0 || !existsSync(outWav)) fail(`SAPI failed: ${(r.stderr || r.error?.message || '').trim().slice(-600)}`);
}

function synthesizePiper(text, outWav) {
  if (!existsSync(PIPER_MODEL)) fail(`Piper voice not found: ${PIPER_MODEL} (see the header of this file)`);
  const input = `${outWav}.txt`;
  writeFileSync(input, text, 'utf8');
  const r = spawnSync(
    PIPER_PYTHON,
    ['-m', 'piper', '-m', PIPER_MODEL, '-i', input, '-f', outWav, '--length-scale', PIPER_LENGTH_SCALE],
    { encoding: 'utf8' },
  );
  if (r.status !== 0 || !existsSync(outWav)) fail(`Piper failed: ${(r.stderr || r.error?.message || '').trim().slice(-600)}`);
}

function synthesizeKokoro(text, outWav) {
  const input = `${outWav}.txt`;
  writeFileSync(input, text, 'utf8');
  const helper = join(import.meta.dirname, 'kokoro-say.py');
  const r = spawnSync(KOKORO_PYTHON, [helper, KOKORO_DIR, KOKORO_VOICE, KOKORO_SPEED, input, outWav], { encoding: 'utf8' });
  if (r.status !== 0 || !existsSync(outWav)) fail(`Kokoro failed: ${(r.stderr || r.error?.message || '').trim().slice(-600)}`);
}

// Writes a 24 kHz mono wav for one paragraph with the selected engine.
function speak(text, voiceNumber, wav) {
  if (ENGINE === 'piper') {
    const raw = `${wav}.piper.wav`;
    synthesizePiper(text, raw);
    toWav(raw, wav);
  } else if (ENGINE === 'kokoro') {
    const raw = `${wav}.kokoro.wav`;
    synthesizeKokoro(text, raw);
    toWav(raw, wav);
  } else if (ENGINE === 'sapi') {
    const raw = `${wav}.sapi.wav`;
    synthesizeSapi(text, voiceNumber === 2 ? SAPI_VOICE_2 : SAPI_VOICE, raw);
    toWav(raw, wav);
  } else {
    const mp3 = `${wav}.mp3`;
    synthesize(text, voiceNumber === 2 ? VOICE_2 : VOICE, mp3);
    toWav(mp3, wav);
  }
}

function synthesize(text, voice, outMp3) {
  for (let attempt = 1; attempt <= 3; attempt++) {
    const r = spawnSync(
      PYTHON,
      ['-m', 'edge_tts', '--voice', voice, `--rate=${RATE}`, '--text', text, '--write-media', outMp3],
      { encoding: 'utf8' },
    );
    if (r.status === 0 && existsSync(outMp3)) return;
    if (attempt === 3) fail(`edge-tts failed: ${(r.stderr || r.error?.message || '').trim().slice(-600)}`);
  }
}

const toWav = (src, dst) =>
  run(FFMPEG, ['-y', '-loglevel', 'error', '-i', src, '-ar', '24000', '-ac', '1', dst]);

const silence = (seconds, dst) =>
  run(FFMPEG, ['-y', '-loglevel', 'error', '-f', 'lavfi', '-i', 'anullsrc=r=24000:cl=mono', '-t', String(seconds), dst]);

const duration = (file) =>
  Number(run(FFPROBE, ['-v', 'error', '-show_entries', 'format=duration', '-of', 'csv=p=0', file]).trim());

// ---------- main ----------

if (args.includes('--say')) {
  const text = args[args.indexOf('--say') + 1];
  const out = args[args.indexOf('--out') + 1] || 'say.wav';
  speak(text, 1, out);
  console.log(`wrote ${out}`);
  process.exit(0);
}

const folderArg = args.find((a) => !a.startsWith('--'));
if (!folderArg) fail('usage: generate-audio.mjs <folder> [--dry-run]');
const folder = resolve(folderArg);
const name = basename(folder);
const script = parseScript(readFileSync(join(folder, 'script.md'), 'utf8'));
const lexiconPath = join(folder, 'pronunciations.json');
const lexicon = existsSync(lexiconPath) ? JSON.parse(readFileSync(lexiconPath, 'utf8')) : {};

if (args.includes('--dry-run')) {
  let total = 0;
  for (const s of script.sections) {
    const words = s.items.reduce((n, i) => n + (i.words || 0), 0);
    const pauses = s.items.reduce((n, i) => n + (i.seconds || 0), 0);
    const seconds = (words / WORDS_PER_MINUTE) * 60 + pauses;
    total += seconds;
    console.log(`${s.title.padEnd(34)} ${String(words).padStart(4)} words  ~${seconds.toFixed(0)}s`);
  }
  console.log(`total ~${(total / 60).toFixed(1)} min at ${WORDS_PER_MINUTE} wpm (edge rate ${RATE} will run a little longer)`);
  process.exit(0);
}

const cache = join(folder, '.cache');
mkdirSync(cache, { recursive: true });

// Kokoro: load the model once and speak every missing paragraph in one process.
if (ENGINE === 'kokoro') {
  const jobs = [];
  let n = 0;
  for (const s of script.sections) {
    for (const item of s.items) {
      n += 1;
      if (item.type === 'pause') continue;
      const spoken = applyLexicon(item.text, lexicon);
      const hash = createHash('sha1').update(`${ENGINE}|${KOKORO_VOICE}|${KOKORO_SPEED}|${spoken}`).digest('hex').slice(0, 8);
      const wav = join(cache, `p${String(n).padStart(3, '0')}-${hash}.wav`);
      if (existsSync(wav)) continue;
      const raw = `${wav}.kokoro.wav`;
      writeFileSync(`${raw}.txt`, spoken, 'utf8');
      jobs.push({ text_file: `${raw}.txt`, out: raw, wav });
    }
  }
  if (jobs.length) {
    console.log(`synthesizing ${jobs.length} paragraphs (kokoro, one process)`);
    const jobsFile = join(cache, 'kokoro-jobs.json');
    writeFileSync(jobsFile, JSON.stringify(jobs), 'utf8');
    const helper = join(import.meta.dirname, 'kokoro-say.py');
    const r = spawnSync(KOKORO_PYTHON, [helper, '--batch', KOKORO_DIR, KOKORO_VOICE, KOKORO_SPEED, jobsFile], { encoding: 'utf8' });
    if (r.status !== 0) fail(`Kokoro failed: ${(r.stderr || r.error?.message || '').trim().slice(-600)}`);
    for (const j of jobs) toWav(j.out, j.wav);
  }
}
const clips = [];
const manifest = { total: 0, sections: [] };
let cursor = 0;
let index = 0;

for (const s of script.sections) {
  const entry = { title: s.title, start: cursor, end: cursor, paragraphs: [] };
  for (const item of s.items) {
    index += 1;
    const id = String(index).padStart(3, '0');
    let wav;
    let seconds;
    if (item.type === 'pause') {
      wav = join(cache, `p${id}-pause-${item.seconds}.wav`);
      if (!existsSync(wav)) silence(item.seconds, wav);
      seconds = duration(wav);
      entry.paragraphs.push({ text: '', speaker: 'pause', words: 0, start: cursor, end: cursor + seconds });
    } else {
      const voice =
        ENGINE === 'piper'
          ? basename(PIPER_MODEL)
          : ENGINE === 'kokoro'
            ? KOKORO_VOICE
            : ENGINE === 'sapi'
              ? item.speaker === 2 ? SAPI_VOICE_2 : SAPI_VOICE
              : item.speaker === 2 ? VOICE_2 : VOICE;
      const rate = ENGINE === 'piper' ? PIPER_LENGTH_SCALE : ENGINE === 'kokoro' ? KOKORO_SPEED : ENGINE === 'sapi' ? SAPI_RATE : RATE;
      const spoken = applyLexicon(item.text, lexicon);
      const hash = createHash('sha1').update(`${ENGINE}|${voice}|${rate}|${spoken}`).digest('hex').slice(0, 8);
      wav = join(cache, `p${id}-${hash}.wav`);
      if (!existsSync(wav)) {
        console.log(`synthesizing ${id} (${ENGINE}): ${spoken.slice(0, 50)}...`);
        speak(spoken, item.speaker, wav);
      }
      seconds = duration(wav);
      entry.paragraphs.push({ text: item.text, speaker: item.speaker, words: item.words, start: cursor, end: cursor + seconds });
    }
    clips.push(wav);
    cursor += seconds;
  }
  entry.end = cursor;
  manifest.sections.push(entry);
}
manifest.total = cursor;

const list = join(cache, 'concat.txt');
writeFileSync(list, clips.map((c) => `file '${c.replace(/\\/g, '/')}'`).join('\n'));
const mp3 = join(folder, `${name}.mp3`);
run(FFMPEG, ['-y', '-loglevel', 'error', '-f', 'concat', '-safe', '0', '-i', list, '-c:a', 'libmp3lame', '-b:a', '128k', mp3]);
writeFileSync(join(cache, 'timing.json'), JSON.stringify(manifest, null, 2));
console.log(`wrote ${mp3} (${(cursor / 60).toFixed(2)} min) and .cache/timing.json`);
