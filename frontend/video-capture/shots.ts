import { Page } from '@playwright/test';
import { join } from 'node:path';

const VIDEOS = join(__dirname, '..', '..', 'docs', 'videos');

/**
 * Save a 1280x800 screenshot as docs/videos/<folder>/images/<name>-1280.png.
 * Pass `clip` to crop to the part of the screen the narration is about.
 */
export async function shot(
  page: Page,
  folder: string,
  name: string,
  clip?: { x: number; y: number; width: number; height: number },
): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.screenshot({ path: join(VIDEOS, folder, 'images', `${name}-1280.png`), clip });
}
