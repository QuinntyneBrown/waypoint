import { expect, test } from '@playwright/test';
import { LandingPage } from '../e2e/pages';
import { clipOf, openWithDemoContent } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '019-public-site-view-journey-and-impact';

test('capture: view journey and impact', async ({ page }) => {
  await openWithDemoContent(page, 'home');
  const landing = new LandingPage(page);
  await expect(landing.heroHeading).toBeVisible();
  await expect(landing.journeyStep('Apply')).toBeVisible();
  await expect(landing.impactStat('Cohorts')).toHaveText('2');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'hero', { x: 0, y: 64, width: 1280, height: 560 });

  const sections = page.locator('main section');
  await shot(page, FOLDER, 'journey', await clipOf(sections.nth(1), 8));
  await shot(page, FOLDER, 'impact', await clipOf(sections.nth(2), 8));
});
