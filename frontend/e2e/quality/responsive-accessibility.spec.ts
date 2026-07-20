import { expect, test } from '@playwright/test';
import { SignInPage, YouthHomePage } from '../pages';
import { registerUser, registerYouth } from '../support/api';
import {
  expectGridColumns,
  expectNoPageOverflow,
  expectNoWcag21AaViolations,
} from '../support/quality';

// Traces to: L2-NFR-009 AC1/AC3; L2-NFR-010 AC1/AC3/AC5
test('authenticated shell is responsive and WCAG 2.1 AA clean', async ({ page, request }) => {
  const viewport = page.viewportSize();
  expect([375, 768, 992, 1200]).toContain(viewport?.width);

  const credentials = await registerYouth(request, `quality-${viewport?.width ?? 'unknown'}`);
  await new SignInPage(page).signIn(credentials);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  await expectNoPageOverflow(page);
  await expectNoWcag21AaViolations(page);
});

// Traces to: L2-NFR-009 AC1/AC3; L2-NFR-010 AC1/AC3/AC5
test('public landing page is responsive and WCAG 2.1 AA clean', async ({ page }) => {
  await page.goto('http://localhost:4201/');
  await expect(page.locator('main')).toBeVisible();

  await expectNoPageOverflow(page);
  await expectNoWcag21AaViolations(page);
});

const NEW_ADMIN_SURFACES = [
  { path: '/admin/dashboard', heading: /welcome back/i },
  { path: '/admin/users-roles', heading: /users & roles/i },
  { path: '/admin/invitations', heading: /invitations/i },
  { path: '/admin/privacy', heading: /privacy requests/i },
  { path: '/help', heading: /how can we help/i },
  { path: '/settings/privacy', heading: /privacy & data rights/i },
] as const;

// Traces to: L2-NFR-009 AC1/AC2/AC3; L2-NFR-010 AC1/AC3/AC5
test('new staff and account surfaces pass the responsive and accessibility matrix', async ({
  page,
  request,
}) => {
  const viewport = page.viewportSize();
  const admin = await registerUser(
    request,
    'ProgramAdministrator',
    `quality-admin-${viewport?.width ?? 'unknown'}`,
  );
  await new SignInPage(page).signIn(admin);

  for (const surface of NEW_ADMIN_SURFACES) {
    await test.step(surface.path, async () => {
      await page.goto(surface.path);
      await expect(page.getByRole('heading', { level: 1, name: surface.heading })).toBeVisible();
      await expect(page.locator('[role="status"]').filter({ hasText: /loading/i })).toHaveCount(0);
      if (surface.path === '/admin/dashboard') {
        const width = viewport?.width ?? 0;
        await expectGridColumns(page, '.dashboard-grid', width <= 768 ? 1 : 2);
        await expectGridColumns(
          page,
          '.dashboard-actions',
          width <= 768 ? 1 : width <= 992 ? 2 : 3,
        );
      }
      await expectNoPageOverflow(page);
      await expectNoWcag21AaViolations(page);
    });
  }
});
