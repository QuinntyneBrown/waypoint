import { expect, test } from '@playwright/test';
import { readFile } from 'node:fs/promises';
import { AdminAuditLogPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-CONSENT-003, L2-REPORT-003, L2-AUTH-009
test('an admin filters and pages the redacted audit trail then exports the current scope', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const privateSecret = `private-audit-${crypto.randomUUID()}`;
  const checkIn = await request.post('/api/mentor/check-ins', {
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
    data: {
      youthId: seeded.youthId,
      date: new Date().toISOString(),
      type: '1:1',
      engagement: 'Engaged',
      goalsDiscussed: privateSecret,
      prayerRequests: privateSecret,
      actionItems: privateSecret,
      shareWithYouthAndGuardian: false,
      guardianInformed: false,
    },
  });
  expect(checkIn.ok(), await checkIn.text()).toBeTruthy();

  const admin = await registerUser(request, 'ProgramAdministrator', 'audit-admin');
  const adminAuth = await signInApi(request, admin);
  for (let index = 0; index < 12; index += 1) {
    const exported = await request.get('/api/admin/audit/export?format=csv', {
      headers: { Authorization: `Bearer ${adminAuth.accessToken}` },
    });
    expect(exported.ok(), await exported.text()).toBeTruthy();
  }

  await new SignInPage(page).signIn(admin);
  const audit = new AdminAuditLogPage(page);
  await audit.goto();

  await expect(audit.heading).toBeVisible();
  await expect(page.getByTestId('audit-integrity-warning')).toHaveCount(0);
  await audit.actionFilter.selectOption('CheckInLogged');
  await audit.applyFilters.click();
  const privateRow = audit.row('CheckInLogged');
  await expect(privateRow).toContainText(seeded.mentorName);
  await expect(privateRow).toContainText(/private/i);
  await expect(privateRow).toContainText('[redacted]');
  await expect(page.getByText(privateSecret)).toHaveCount(0);

  const today = new Date().toISOString().slice(0, 10);
  await audit.userFilter.selectOption(admin.userId);
  await audit.actionFilter.selectOption('DataExported');
  await audit.from.fill(today);
  await audit.to.fill(today);
  await audit.applyFilters.click();

  await expect(audit.summary).toContainText('12 events');
  await expect(audit.rows).toHaveCount(10);
  await audit.nextPage.click();
  await expect(audit.summary).toContainText('Page 2 of 2');
  await expect(audit.rows).toHaveCount(2);
  await audit.previousPage.click();

  const [download] = await Promise.all([page.waitForEvent('download'), audit.exportCsv.click()]);
  expect(download.suggestedFilename()).toMatch(/audit-log.*\.csv$/i);
  const path = await download.path();
  expect(path).not.toBeNull();
  const csv = await readFile(path!, 'utf8');
  expect(csv).toContain('DataExported');
  expect(csv).toContain(admin.userId);
  expect(csv).not.toContain('CheckInLogged');

  await audit.from.fill('2099-01-01');
  await audit.to.fill('2099-01-02');
  await audit.applyFilters.click();
  await expect(page.getByRole('heading', { name: 'No activity for this filter' })).toBeVisible();
  await audit.clearFilters.click();
  await expect(audit.rows.first()).toBeVisible();
});

// Traces to: L2-CONSENT-003 AC3
test('an admin is warned when audit-chain verification fails', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'audit-integrity-admin');
  await new SignInPage(page).signIn(admin);
  await page.route('**/api/audit?**', async (route) => {
    await route.fulfill({
      json: {
        items: [],
        page: 1,
        pageSize: 10,
        total: 0,
        actors: [],
        actions: [],
        integrityValid: false,
      },
    });
  });

  await new AdminAuditLogPage(page).goto();

  await expect(page.getByTestId('audit-integrity-warning')).toContainText(
    'Audit integrity check failed',
  );
});
