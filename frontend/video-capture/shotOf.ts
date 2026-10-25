import { join } from 'node:path';
import { Locator, Page } from '@playwright/test';
import { shot } from './shots';

/** Crop a screenshot to an element with padding, clamped to the 1280x800 viewport. */
export async function shotOf(
  page: Page,
  folder: string,
  name: string,
  target: Locator,
  pad = 16,
  maxWidth = 1280,
  maxHeight = 800,
): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  const box = (await target.boundingBox())!;
  const x = Math.max(0, box.x - pad);
  const y = Math.max(0, box.y - pad);
  await shot(page, folder, name, {
    x,
    y,
    width: Math.min(maxWidth, 1280 - x, box.width + pad * 2),
    height: Math.min(maxHeight, 800 - y, box.height + pad * 2),
  });
}

/** Crop a full-page screenshot (document coordinates), for regions taller than the viewport. */
export async function shotFull(
  page: Page,
  folder: string,
  name: string,
  clip: { x: number; y: number; width: number; height: number },
): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 800 });
  // Document coordinates: from the top, so the sticky top bar stays at the top.
  await page.evaluate(() => window.scrollTo(0, 0));
  await page.screenshot({
    path: join(__dirname, '..', '..', 'docs', 'videos', folder, 'images', `${name}-1280.png`),
    fullPage: true,
    clip,
  });
}
