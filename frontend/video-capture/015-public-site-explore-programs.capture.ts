import { expect, test } from '@playwright/test';
import { ProgramsPage } from '../e2e/pages';
import { clipOf, openWithDemoContent } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '015-public-site-explore-programs';

test('capture: explore programs', async ({ page }) => {
  await openWithDemoContent(page, 'Programs');
  const programs = new ProgramsPage(page);
  await expect(programs.track('Bible & Leadership')).toBeVisible();
  await expect(programs.track('Coding for Kingdom')).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'programs', { x: 0, y: 70, width: 1280, height: 730 });

  await shot(page, FOLDER, 'tracks', await clipOf(page.locator('.w-grid--2'), 12));
  await shot(page, FOLDER, 'mentorship', await clipOf(page.locator('section').nth(1), 8));
});
