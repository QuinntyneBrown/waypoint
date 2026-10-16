import { expect, test } from '@playwright/test';
import { AdminPrivacyPage, PrivacySettingsPage } from '../e2e/pages';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';
import { shotOf } from './shotOf';

const FOLDER = '109-non-functional-protect-pii-and-data-rights';

test.setTimeout(240_000);

test('capture: data rights for a youth and for staff', async ({ page, browser, request }) => {
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const made = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Amara Bell', role: 'Youth' },
  });
  expect(made.ok()).toBeTruthy();
  const adminEmail = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const madeAdmin = await request.post('/api/_test/users', {
    data: { email: adminEmail, password, displayName: 'Pat Morgan', role: 'ProgramAdministrator' },
  });
  expect(madeAdmin.ok()).toBeTruthy();

  await signInSlow(page, { email, password });
  await page.setViewportSize({ width: 1280, height: 800 });
  const privacy = new PrivacySettingsPage(page);
  await privacy.goto();
  await expect(privacy.submitButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'privacy-page', { x: 380, y: 80, width: 760, height: 720 });

  await privacy.action.selectOption('access');
  await privacy.details.fill('Please provide a copy for my records.');
  await privacy.submitButton.click();
  await expect(page.getByRole('status')).toContainText(/request received.*reference/i);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'privacy-received', { x: 380, y: 300, width: 760, height: 330 });

  const staff = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const staffPage = await staff.newPage();
  await signInSlow(staffPage, { email: adminEmail, password });
  await staffPage.setViewportSize({ width: 1280, height: 800 });
  const adminPrivacy = new AdminPrivacyPage(staffPage);
  await adminPrivacy.goto();
  await expect(adminPrivacy.heading).toBeVisible();
  await staffPage.waitForLoadState('networkidle');
  const mine = staffPage.getByTestId('privacy-request').filter({ hasText: email });
  for (let pageNumber = 0; pageNumber < 12 && (await mine.count()) === 0; pageNumber += 1) {
    await staffPage.getByRole('button', { name: 'Next', exact: true }).click();
    await staffPage.waitForLoadState('networkidle');
  }
  await expect(mine).toHaveCount(1);
  await staffPage.mouse.move(0, 0);
  await shotOf(staffPage, FOLDER, 'admin-request', mine, 24, 900, 400);
});
