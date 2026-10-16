import { expect, test } from '@playwright/test';
import { SignInPage, SupportPage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '096-platform-services-submit-support-request';
const FORM = { x: 362, y: 56, width: 800, height: 700 };

test('capture: support request', async ({ page, request }) => {
  const email = `david.osei.${crypto.randomUUID().slice(0, 4)}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: 'Password123', displayName: 'David Osei', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();
  await new SignInPage(page).signIn({ email, password: 'Password123' });

  const support = new SupportPage(page);
  await support.goto();
  await expect(support.sendButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'form', FORM);

  await support.message.fill('too short');
  await support.sendButton.click();
  await expect(page.getByText(/at least 10 characters/i)).toBeVisible();
  await shot(page, FOLDER, 'required', FORM);

  await support.message.fill('I cannot find the lesson my mentor assigned me this week.');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'filled', FORM);
  await support.sendButton.click();
  await expect(support.acknowledgementHeading).toBeVisible();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'received', { ...FORM, height: 360 });
});
