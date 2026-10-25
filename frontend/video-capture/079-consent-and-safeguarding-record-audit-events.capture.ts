import { expect, test } from '@playwright/test';
import { AdminAuditLogPage, SignInPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedCast } from './mentoring-cast';
import { shotOf } from './shotOf';

const FOLDER = '079-consent-and-safeguarding-record-audit-events';

test('capture: record audit events', async ({ page, request }) => {
  test.setTimeout(180_000);
  const seeded = await seedCast(request, 'Amara Bell');
  // Grace signs in and logs a private check-in, so both actions are written to the audit trail.
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

  await new SignInPage(page).signIn({ email, password: 'Password123' });
  const audit = new AdminAuditLogPage(page);
  await audit.goto();
  await expect(audit.heading).toBeVisible();
  await audit.userFilter.selectOption(mentorAuth.userId);
  await audit.applyFilters.click();
  await expect(audit.rows.first()).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shotOf(page, FOLDER, 'mentor-events', page.locator('table.ph-table'), 12);
  console.log('ROWS', JSON.stringify(await audit.rows.allInnerTexts()));
});
