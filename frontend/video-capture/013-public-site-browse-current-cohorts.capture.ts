import { expect, test } from '@playwright/test';
import { AboutPage } from '../e2e/pages';
import { clipOf, openWithDemoContent } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '013-public-site-browse-current-cohorts';

test('capture: browse current cohorts', async ({ page }) => {
  await openWithDemoContent(page, 'About');
  const about = new AboutPage(page);
  await expect(about.cohort('Regent Park Fall Cohort')).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'about', { x: 0, y: 70, width: 1280, height: 640 });

  const cards = page.getByRole('article');
  await shot(page, FOLDER, 'cohorts', await clipOf(page.locator('.ph-grid--2'), 12));
  await expect(cards).toHaveCount(2);

  // The empty state, shown with the same page when no cohort is listed.
  await page.unroute('**/api/public/content');
  await page.route('**/api/public/content', async (route) => {
    const res = await route.fetch();
    const json = await res.json();
    json.cohorts = [];
    await route.fulfill({ response: res, json });
  });
  await page
    .getByRole('navigation', { name: 'Primary' })
    .getByRole('link', { name: 'Contact' })
    .click();
  await page
    .getByRole('navigation', { name: 'Primary' })
    .getByRole('link', { name: 'About' })
    .click();
  await expect(
    page.getByRole('heading', { name: 'No cohorts are currently listed' }),
  ).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'empty', await clipOf(page.locator('.ph-empty'), 40));
});
