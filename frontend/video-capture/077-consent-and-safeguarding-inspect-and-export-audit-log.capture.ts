import { expect, test } from '@playwright/test';
import { AdminAuditLogPage, SignInPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedCast } from './mentoring-cast';
import { settle } from './guardian-cast';
import { shotOf } from './shotOf';
import { shot } from './shots';

const FOLDER = '077-consent-and-safeguarding-inspect-and-export-audit-log';

test('capture: inspect and export the audit log', async ({ page, request }) => {
  test.setTimeout(180_000);
  const seeded = await seedCast(request, 'Amara Bell');
  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const checkIn = await request.post('/api/mentor/check-ins', {
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
    data: {
      youthId: seeded.youthId,
      date: new Date().toISOString(),
      type: '1:1',
      engagement: 'Engaged',
      goalsDiscussed: 'Private conversation notes',
      prayerRequests: 'Private conversation notes',
      actionItems: 'Private conversation notes',
      shareWithYouthAndGuardian: false,
      guardianInformed: false,
    },
  });
  expect(checkIn.ok(), await checkIn.text()).toBeTruthy();

  const email = `pat-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const created = await request.post('/api/_test/users', {
    data: {
      email,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(created.ok()).toBeTruthy();
  const { userId } = (await created.json()) as { userId: string };
  const adminAuth = await signInApi(request, { email, password: 'Password123' });
  for (let i = 0; i < 3; i += 1) {
    const exported = await request.get('/api/admin/audit/export?format=csv', {
      headers: { Authorization: `Bearer ${adminAuth.accessToken}` },
    });
    expect(exported.ok()).toBeTruthy();
  }

  await new SignInPage(page).signIn({ email, password: 'Password123' });
  const audit = new AdminAuditLogPage(page);
  await audit.goto();
  await expect(audit.heading).toBeVisible();
  // Fail (and retry) if the portal's button styles have not loaded yet.
  await expect(audit.applyFilters).not.toHaveCSS('background-color', 'rgba(0, 0, 0, 0)');
  await expect(audit.exportCsv).not.toHaveCSS('background-color', 'rgba(0, 0, 0, 0)');
  await expect(page.getByTestId('audit-integrity-warning')).toHaveCount(0);
  await page.waitForLoadState('networkidle');
  await settle(page);
  await shotOf(page, FOLDER, 'overview', page.locator('.w-container.w-page'), 0, 1280, 400);

  await audit.actionFilter.selectOption('CheckInLogged');
  await audit.userFilter.selectOption(mentorAuth.userId);
  await audit.applyFilters.click();
  await expect(audit.row('CheckInLogged').first()).toContainText('[redacted]');
  await settle(page);
  await shotOf(page, FOLDER, 'redacted', page.locator('.w-container.w-page'), 0, 1280, 620);

  await audit.userFilter.selectOption(userId);
  await audit.actionFilter.selectOption('DataExported');
  await audit.applyFilters.click();
  await expect(audit.summary).toContainText('3 events');
  await settle(page);
  await shotOf(page, FOLDER, 'filtered', page.locator('.w-container.w-page'), 0);

  await audit.exportCsv.click();
  await expect(page.locator('.w-alert--success')).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await settle(page);
  await shot(page, FOLDER, 'exported', { x: 244, y: 56, width: 1036, height: 330 });
});
