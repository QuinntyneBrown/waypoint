import { expect, test } from '@playwright/test';
import { ForbiddenPage, PeopleDirectoryPage, SignInPage, YouthHomePage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '091-platform-services-recover-from-forbidden-access';
const STATE = { x: 340, y: 230, width: 600, height: 340 };

test('capture: forbidden', async ({ page, request }) => {
  const email = `david.osei.${crypto.randomUUID().slice(0, 4)}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: 'Password123', displayName: 'David Osei', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();
  await new SignInPage(page).signIn({ email, password: 'Password123' });
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  await new PeopleDirectoryPage(page).goto();
  const forbidden = new ForbiddenPage(page);
  await expect(forbidden.heading).toBeVisible();
  await expect(page).toHaveURL(/\/forbidden$/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'forbidden', STATE);

  await page.getByRole('link', { name: /back to your home/i }).click();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'home');
});
