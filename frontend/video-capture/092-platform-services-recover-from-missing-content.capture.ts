import { expect, test } from '@playwright/test';
import { NotFoundPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '092-platform-services-recover-from-missing-content';
const STATE = { x: 340, y: 230, width: 600, height: 340 };

test('capture: not found', async ({ page, request }) => {
  const email = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: {
      email,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(seeded.ok()).toBeTruthy();
  await new SignInPage(page).signIn({ email, password: 'Password123' });

  const notFound = new NotFoundPage(page);
  await notFound.goto('/admin/cohorts/old-cohort-page');
  await expect(notFound.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'not-found', STATE);

  await notFound.backToSafetyLink.click();
  await expect(page).toHaveURL(/\/admin/);
  await expect(page.getByText(/loading the director dashboard/i)).toBeHidden({ timeout: 15_000 });
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'safe-home', { x: 0, y: 0, width: 1280, height: 210 });
});
