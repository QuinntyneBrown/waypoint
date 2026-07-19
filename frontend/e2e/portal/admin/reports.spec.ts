import { expect, test } from '@playwright/test';
import { readFile } from 'node:fs/promises';
import { AdminReportsPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-REPORT-001, L2-AUTH-010
test('an admin filters complete organization, youth, cohort, attendance, and staff reports', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'reports-admin');
  await new SignInPage(page).signIn(admin);
  const reports = new AdminReportsPage(page);
  await reports.goto();

  await expect(reports.heading).toBeVisible();
  const from = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);
  const to = new Date().toISOString().slice(0, 10);
  await reports.from.fill(from);
  await reports.to.fill(to);
  await reports.cohortFilter.selectOption(seeded.cohortId);
  await reports.trackFilter.selectOption(seeded.primaryTrackId);
  await reports.mentorFilter.selectOption(seeded.mentorId);
  await reports.applyFilters.click();

  await expect(reports.updatedAt).toContainText(/updated/i);
  await expect(reports.recordCount).toContainText('1 youth record');
  await expect(reports.overview).toContainText('Active youth');
  await expect(reports.overview).toContainText('Attendance');
  await expect(reports.overview).toContainText('Present');
  await expect(reports.overview).toContainText('Absent');

  await reports.youthTab.click();
  await reports.youth.selectOption(seeded.youthId);
  await reports.generateYouth.click();
  await expect(reports.youthReport).toContainText(seeded.youthName);
  await expect(reports.youthReport).toContainText(seeded.firstModuleName);
  await expect(reports.youthReport).toContainText('50% complete');
  await expect(reports.youthReport).toContainText(seeded.progressQuizTitle);
  await expect(reports.youthReport).toContainText('82%');
  await expect(reports.youthReport).toContainText('Present 1');
  await expect(reports.youthReport).toContainText('Absent 1');
  await expect(reports.youthReport).toContainText(seeded.progressCompetencyName);
  await expect(reports.youthReport).toContainText('Proficient');
  await expect(reports.youthReport).toContainText('1 mentor check-in');
  await expect(reports.youthReport).toContainText(seeded.journeyBadgeName);

  await reports.cohortTab.click();
  await reports.reportCohort.selectOption(seeded.cohortId);
  await reports.generateCohort.click();
  await expect(reports.cohortReport).toContainText(seeded.cohortName);
  await expect(reports.cohortReport).toContainText('1 enrolled');
  await expect(reports.cohortReport).toContainText(seeded.firstModuleName);
  await expect(reports.cohortReport).toContainText('At risk 1');
  await expect(reports.cohortReport).toContainText('Program average');

  await reports.staffTab.click();
  const mentor = reports.staffRow(seeded.mentorName);
  await expect(mentor).toContainText('Caseload 1');
  await expect(mentor).toContainText('Check-ins 1');
  await expect(mentor).toContainText('Under target');
});

// Traces to: L2-REPORT-001 AC5
test('the reports hub shows a real empty scope and recovers from a load failure', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'reports-states');
  const auth = await signInApi(request, admin);
  const cohortResponse = await request.post('/api/cohorts', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: {
      name: `Empty report cohort ${crypto.randomUUID().slice(0, 8)}`,
      trackId: seeded.primaryTrackId,
      leadMentorId: seeded.mentorId,
      startDate: new Date().toISOString(),
      endDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000).toISOString(),
      location: 'Reporting room',
    },
  });
  expect(cohortResponse.ok(), await cohortResponse.text()).toBeTruthy();
  const emptyCohort = (await cohortResponse.json()) as { id: string };

  await new SignInPage(page).signIn(admin);
  const reports = new AdminReportsPage(page);
  await reports.goto();
  await reports.cohortFilter.selectOption(emptyCohort.id);
  await reports.applyFilters.click();
  await expect(
    page.getByRole('heading', { name: 'No report data for these filters' }),
  ).toBeVisible();

  await page.route(
    '**/api/reports/hub?*',
    (route) => route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{}' }),
    { times: 1 },
  );
  await reports.applyFilters.click();
  await expect(page.getByRole('heading', { name: 'Something went wrong' })).toBeVisible();
  await page.getByRole('button', { name: 'Try again', exact: true }).click();
  await expect(reports.heading).toBeVisible();
  await expect(reports.updatedAt).toContainText(/updated/i);
});

// Traces to: L2-REPORT-003, L2-AUTH-010, L2-CONSENT-003
test('report downloads preserve the current view and filters in CSV and PDF', async ({
  page,
  request,
}) => {
  const included = await seedYouthCurriculum(request);
  const excluded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'report-export');
  await new SignInPage(page).signIn(admin);
  const reports = new AdminReportsPage(page);
  await reports.goto();

  await reports.cohortFilter.selectOption(included.cohortId);
  await reports.trackFilter.selectOption(included.primaryTrackId);
  await reports.mentorFilter.selectOption(included.mentorId);
  await reports.applyFilters.click();

  const [csv] = await Promise.all([page.waitForEvent('download'), reports.exportCsv.click()]);
  expect(csv.suggestedFilename()).toMatch(/overview-report.*\.csv$/i);
  const csvPath = await csv.path();
  expect(csvPath).not.toBeNull();
  const csvText = await readFile(csvPath!, 'utf8');
  expect(csvText).toContain(included.cohortName);
  expect(csvText).toContain(included.mentorName);
  expect(csvText).not.toContain(excluded.cohortName);

  await reports.youthTab.click();
  await reports.youth.selectOption(included.youthId);
  await reports.generateYouth.click();
  const [pdf] = await Promise.all([page.waitForEvent('download'), reports.exportPdf.click()]);
  expect(pdf.suggestedFilename()).toMatch(/youth-report.*\.pdf$/i);
  const pdfPath = await pdf.path();
  expect(pdfPath).not.toBeNull();
  const pdfBytes = await readFile(pdfPath!);
  expect(pdfBytes.subarray(0, 8).toString('ascii')).toBe('%PDF-1.4');
});
