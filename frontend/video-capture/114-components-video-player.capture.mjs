// Captures the ph-video-player Storybook stories in light and dark for video 114.
// Run from frontend/ after `npm run build-storybook`:
//   node video-capture/114-components-video-player.capture.mjs
// Needs Microsoft Edge (H.264 and AAC); Playwright's bundled Chromium cannot play them.
import { chromium } from '@playwright/test';
import { createServer } from 'node:http';
import { mkdirSync, readFile } from 'node:fs';
import { extname, join } from 'node:path';

const ROOT = join(import.meta.dirname, '..', 'dist', 'storybook', 'components');
const OUT = join(
  import.meta.dirname,
  '..',
  '..',
  'docs',
  'videos',
  '114-components-video-player',
  'images',
);
mkdirSync(OUT, { recursive: true });

const types = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.svg': 'image/svg+xml',
  '.woff2': 'font/woff2',
  '.vtt': 'text/vtt',
  '.fmp4': 'video/mp4',
};
const server = createServer((req, res) => {
  const file = join(ROOT, decodeURIComponent(req.url.split('?')[0]));
  readFile(file, (error, data) => {
    if (error) {
      res.writeHead(404);
      res.end();
      return;
    }
    res.writeHead(200, { 'content-type': types[extname(file)] ?? 'application/octet-stream' });
    res.end(data);
  });
}).listen(6007);

const browser = await chromium.launch({
  channel: 'msedge',
  args: ['--autoplay-policy=no-user-gesture-required'],
});

async function open(page, story, theme) {
  await page.goto(
    `http://localhost:6007/iframe.html?id=components-video-player--${story}&viewMode=story`,
  );
  await page.evaluate((t) => document.documentElement.setAttribute('data-theme', t), theme);
  await page.waitForSelector('ph-video-player');
  return page.locator('ph-video-player');
}
const state = (player) => player.getAttribute('data-state');
async function waitState(player, expected, timeout = 20000) {
  await player.and(player.page().locator(`[data-state="${expected}"]`)).waitFor({ timeout });
}
async function snap(page, name, theme, locator) {
  await page.waitForTimeout(400);
  await (locator ?? page.locator('ph-video-player')).screenshot({
    path: join(OUT, `${name}-${theme}.png`),
  });
}

for (const theme of ['light', 'dark']) {
  const context = await browser.newContext({
    viewport: { width: 860, height: 520 },
    deviceScaleFactor: 2,
    colorScheme: theme,
    reducedMotion: 'no-preference',
  });
  const page = await context.newPage();
  page.on('pageerror', (e) => console.error('pageerror', e.message));

  let player = await open(page, 'idle', theme);
  await snap(page, 'idle', theme);

  player = await open(page, 'connecting', theme);
  await snap(page, 'connecting', theme);

  player = await open(page, 'live', theme);
  await waitState(player, 'live');
  await page.waitForTimeout(1500);
  await page.locator('ph-video-player').hover();
  await snap(page, 'live', theme);

  await page.locator('[data-control="play-pause"]').click();
  await waitState(player, 'paused');
  await snap(page, 'paused', theme);
  await page.keyboard.press('k');
  await waitState(player, 'live');

  await page.locator('[data-control="volume"]').focus();
  await snap(page, 'volume', theme);

  await page.locator('[data-control="mute"]').click();
  await snap(page, 'muted', theme);

  player = await open(page, 'with-captions', theme);
  await waitState(player, 'live');
  await page.locator('[data-control="captions"]').click();
  await page.waitForTimeout(1500);
  await snap(page, 'captions', theme);

  player = await open(page, 'live', theme);
  await waitState(player, 'live');
  await page.evaluate(() => (window.wDemo.stalled = true));
  await waitState(player, 'buffering', 30000);
  await snap(page, 'buffering', theme);
  await page.evaluate(() => (window.wDemo.stalled = false));
  await waitState(player, 'live');
  await page.evaluate(() => window.wDemo.connectionEvents.next('reconnecting'));
  await waitState(player, 'reconnecting');
  await snap(page, 'reconnecting', theme);
  await page.evaluate(() => window.wDemo.connectionEvents.next('reconnected'));
  await page.evaluate(() => window.wDemo.end());
  await waitState(player, 'ended');
  await snap(page, 'ended', theme);

  player = await open(page, 'unauthorized', theme);
  await waitState(player, 'error');
  await snap(page, 'unauthorized', theme);

  player = await open(page, 'localised', theme);
  await waitState(player, 'live');
  await snap(page, 'localised', theme);

  player = await open(page, 'themed', theme);
  await waitState(player, 'live');
  await snap(page, 'themed', theme);

  player = await open(page, 'live', theme);
  await waitState(player, 'live');
  await page.keyboard.press('Tab');
  await snap(page, 'focus', theme);

  await player.evaluate((el) => el.requestFullscreen());
  await page.waitForTimeout(800);
  await page.screenshot({ path: join(OUT, `fullscreen-${theme}.png`) });
  await context.close();
}
await browser.close();
server.close();
console.log('captured to', OUT);
