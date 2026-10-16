import { expect, Page, test } from '@playwright/test';
import { join } from 'node:path';
import { SignInPage, YouthHomePage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '103-non-functional-adapt-layouts';

test.setTimeout(240_000);

/** Save a screenshot of the page at its current (narrow) viewport. */
async function xs(page: Page, name: string, height = 760): Promise<void> {
  await page.mouse.move(0, 0);
  await page.screenshot({
    path: join(__dirname, '..', '..', 'docs', 'videos', FOLDER, 'images', `${name}-1280.png`),
    clip: { x: 0, y: 0, width: 375, height },
  });
}

test('capture: youth home wide and narrow', async ({ page, browser, request }) => {
  const seeded = await seedEngagementCast(request);
  const creds = { email: seeded.youthEmail, password: seeded.youthPassword };

  await signInSlow(page, creds);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'youth-wide', { x: 0, y: 0, width: 1280, height: 800 });

  const narrow = await browser.newContext({
    baseURL: 'http://localhost:4200',
    viewport: { width: 375, height: 800 },
  });
  const phone = await narrow.newPage();
  await signInSlow(phone, creds);
  await expect(phone).toHaveURL(YouthHomePage.urlPattern);
  await phone.waitForLoadState('networkidle');
  await xs(phone, 'youth-narrow', 800);
  await narrow.close();
});

test('capture: staff pages wide and narrow', async ({ page, browser, request }) => {
  await seedEngagementCast(request);
  const admin = { email: `pat-${crypto.randomUUID()}@waypoint.demo`, password: 'Password123' };
  const made = await request.post('/api/_test/users', {
    data: { ...admin, displayName: 'Pat Morgan', role: 'ProgramAdministrator' },
  });
  expect(made.ok()).toBeTruthy();
  await signInSlow(page, admin);
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto('/admin/dashboard');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'admin-wide', { x: 0, y: 0, width: 1280, height: 800 });

  const narrow = await browser.newContext({
    baseURL: 'http://localhost:4200',
    viewport: { width: 375, height: 800 },
  });
  const phone = await narrow.newPage();
  await signInSlow(phone, admin);
  for (const [path, name] of [
    ['/admin/dashboard', 'admin-narrow'],
    ['/admin/people', 'people-narrow'],
    ['/admin/cohort-progress', 'progress-narrow'],
  ]) {
    await phone.goto(path);
    await phone.waitForLoadState('networkidle');
    await phone.waitForTimeout(800);
    if (name === 'people-narrow') await phone.evaluate(() => window.scrollTo(0, 560));
    await xs(phone, name, 800);
  }
  await narrow.close();
});
