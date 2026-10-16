import { expect, test } from '@playwright/test';
import { join } from 'node:path';
import { LandingPage } from '../e2e/pages';
import { clipOf } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '016-public-site-navigate-public-site';

test('capture: navigate the public site', async ({ page, browser }) => {
  const landing = new LandingPage(page);
  await landing.goto();
  await expect(landing.heroHeading).toBeVisible();
  await shot(page, FOLDER, 'landing', { x: 0, y: 0, width: 1280, height: 640 });

  await page.getByRole('link', { name: 'Privacy' }).click();
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  await shot(page, FOLDER, 'footer', await clipOf(page.locator('footer'), 0));

  await page.goto('/no-such-page', { waitUntil: 'networkidle' });
  await expect(page.getByRole('heading', { name: /couldn.t find that page/i })).toBeVisible();
  await shot(page, FOLDER, 'not-found', await clipOf(page.locator('main section'), 24));

  const phone = await browser.newPage({
    baseURL: 'http://localhost:4201',
    viewport: { width: 375, height: 800 },
  });
  await phone.goto('/', { waitUntil: 'networkidle' });
  await expect(phone.getByRole('heading', { name: /young leaders who love jesus/i })).toBeVisible();
  await phone.screenshot({
    path: join(__dirname, '..', '..', 'docs', 'videos', FOLDER, 'images', 'phone-1280.png'),
  });

  await landing.goto();
  await landing.signInLink.click();
  await expect(page).toHaveURL('http://localhost:4200/auth/sign-in');
  await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
  await shot(page, FOLDER, 'sign-in', { x: 380, y: 90, width: 520, height: 620 });
});
