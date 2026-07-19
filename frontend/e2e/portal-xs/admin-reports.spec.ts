import { expect, test } from '@playwright/test';
import { AdminReportsPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-REPORT-001, L2-NFR-009, L2-NFR-010
test('reports filters and detail views remain touch usable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'reports-mobile');
  await new SignInPage(page).signIn(admin);
  const reports = new AdminReportsPage(page);
  await reports.goto();

  await expect(reports.heading).toBeVisible();
  await reports.cohortFilter.selectOption(seeded.cohortId);
  expect((await reports.applyFilters.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await reports.applyFilters.click();
  await reports.youthTab.click();
  await reports.youth.selectOption(seeded.youthId);
  expect((await reports.generateYouth.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await reports.generateYouth.click();
  await expect(reports.youthReport).toContainText(seeded.youthName);
  await reports.staffTab.click();
  await expect(reports.staffRow(seeded.mentorName)).toBeVisible();
  await expect.poll(() => reports.horizontalOverflow()).toBeLessThanOrEqual(1);
});
