import { expect, test } from '@playwright/test';
import { ServerErrorPage, SignInPage, SupportPage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '093-platform-services-recover-from-server-error';
const STATE = { x: 340, y: 230, width: 600, height: 340 };
const FORM = { x: 362, y: 56, width: 800, height: 700 };
const DRAFT = 'The lesson page will not open for me. I tried twice this morning.';

test('capture: server error keeps the draft', async ({ page, request }) => {
  const email = `david.osei.${crypto.randomUUID().slice(0, 4)}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email, password: 'Password123', displayName: 'David Osei', role: 'Youth' },
  });
  expect(seeded.ok()).toBeTruthy();
  await new SignInPage(page).signIn({ email, password: 'Password123' });

  const support = new SupportPage(page);
  await support.goto();
  await support.message.fill(DRAFT);
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'draft', FORM);

  await page.route('**/api/support', (route) =>
    route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: '{"title":"An unexpected error occurred."}',
    }),
  );
  await support.sendButton.click();
  const serverError = new ServerErrorPage(page);
  await expect(serverError.heading).toBeVisible();
  await expect(page).toHaveURL(/\/server-error$/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'server-error', STATE);

  await serverError.retryButton.click();
  await expect(page).toHaveURL(/\/support$/);
  await expect(support.message).toHaveValue(DRAFT);
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'restored', FORM);
});
