import { APIRequestContext, Locator, Page, expect } from '@playwright/test';
import { join } from 'node:path';
import { SignInPage } from '../e2e/pages';

const VIDEOS = join(__dirname, '..', '..', 'docs', 'videos');

/** Seed Pat Morgan, a program administrator, and sign in through the real page. */
export async function signInAsPat(page: Page, request: APIRequestContext): Promise<void> {
  const email = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Pat Morgan', role: 'ProgramAdministrator' },
  });
  expect(res.ok()).toBeTruthy();
  await new SignInPage(page).signIn({ email, password });
}

/** Save a screenshot of one element (a dialog or a card) next to the video. */
export async function shotOf(
  target: Locator,
  folder: string,
  name: string,
  pad = 0,
): Promise<void> {
  const page = target.page();
  await page.setViewportSize({ width: 1280, height: 1000 });
  await page.evaluate(() => window.scrollTo(0, 0));
  const inView = await target.evaluate((el) => el.getBoundingClientRect().bottom < innerHeight);
  if (!inView) await target.scrollIntoViewIfNeeded();
  const box = await target.boundingBox();
  if (!box) throw new Error('no box for ' + name);
  const x = Math.max(0, box.x - pad);
  const y = Math.max(0, box.y - pad);
  await page.screenshot({
    path: join(VIDEOS, folder, 'images', `${name}-1280.png`),
    clip: { x, y, width: Math.min(1280 - x, box.width + 2 * pad), height: box.height + 2 * pad },
  });
}

/** Save a screenshot of the area that covers several elements. */
export async function shotAround(
  targets: Locator[],
  folder: string,
  name: string,
  pad = 8,
): Promise<void> {
  const page = targets[0].page();
  await page.setViewportSize({ width: 1280, height: 1000 });
  const boxes = [];
  for (const target of targets) {
    const box = await target.boundingBox();
    if (!box) throw new Error('no box for ' + name);
    boxes.push(box);
  }
  const x = Math.max(0, Math.min(...boxes.map((b) => b.x)) - pad);
  const y = Math.max(0, Math.min(...boxes.map((b) => b.y)) - pad);
  const right = Math.max(...boxes.map((b) => b.x + b.width)) + pad;
  const bottom = Math.max(...boxes.map((b) => b.y + b.height)) + pad;
  await page.screenshot({
    path: join(VIDEOS, folder, 'images', `${name}-1280.png`),
    clip: { x, y, width: Math.min(1280 - x, right - x), height: bottom - y },
  });
}
