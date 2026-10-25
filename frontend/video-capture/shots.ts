import { Page } from '@playwright/test';
import { join } from 'node:path';

const VIDEOS = join(__dirname, '..', '..', 'docs', 'videos');

interface Clip {
  x: number;
  y: number;
  width: number;
  height: number;
}

/**
 * Save a 1280x800 screenshot as docs/videos/<folder>/images/<name>-1280.png.
 * Pass `clip` to crop to the part of the screen the narration is about; its
 * bottom edge is moved down past any control or line of text it would cut.
 */
export async function shot(page: Page, folder: string, name: string, clip?: Clip): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.screenshot({
    path: join(VIDEOS, folder, 'images', `${name}-1280.png`),
    clip: clip && (await fitBottom(page, clip)),
  });
}

/**
 * Crops were sized by hand; when the UI grows they slice through the last
 * row of controls. Extend the bottom edge (by up to 240px, within the
 * viewport) until no control, image or line of text crosses it.
 */
export async function fitBottom(page: Page, clip: Clip): Promise<Clip> {
  const bottom = await page.evaluate((c) => {
    const ATOMIC =
      'button, input, select, textarea, img, svg, label, a, h1, h2, h3, h4, h5, h6, p, li, td, th, dt, dd, span';
    const limit = Math.min(window.innerHeight, c.y + c.height + 240);
    let edge = c.y + c.height;
    for (let pass = 0; pass < 6; pass++) {
      let next = edge;
      for (let x = c.x + 4; x < c.x + c.width; x += 12) {
        for (const el of document.elementsFromPoint(x, edge - 1)) {
          if (!el.matches(ATOMIC)) continue;
          const r = el.getBoundingClientRect();
          if (r.top < edge - 1 && r.bottom > edge && r.height < 200)
            next = Math.max(next, r.bottom + 8);
        }
      }
      next = Math.min(next, limit);
      if (next === edge) break;
      edge = next;
    }
    return edge;
  }, clip);
  return { ...clip, height: Math.max(clip.height, Math.round(bottom - clip.y)) };
}
