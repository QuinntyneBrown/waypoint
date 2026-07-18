import { expect, test } from '@playwright/test';
import { ProgressBoardPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ADMIN-004, L2-NFR-005
test('the selected progress matrix refreshes live when youth progress changes', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'progress-live');
  await new SignInPage(page).signIn(admin);

  const board = new ProgressBoardPage(page);
  await board.goto();
  await board.cohortSelect.selectOption(seeded.cohortId);
  await board.trackSelect.selectOption(seeded.primaryTrackId);
  const row = board.row(seeded.youthName);
  await expect(row).toContainText(seeded.firstModuleName);
  await expect(row).toContainText('In progress');
  await expect(board.liveStatus).toContainText('Live');

  const youthAuth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const complete = await request.post(`/api/me/lessons/${seeded.secondLessonId}/complete`, {
    headers: { Authorization: `Bearer ${youthAuth.accessToken}` },
  });
  expect(complete.ok(), await complete.text()).toBeTruthy();

  await expect(row).toContainText('Done');
  await expect(row).toContainText('100%');
});

// Traces to: L2-ADMIN-004 AC3, L2-REPORT-003
test('CSV and PDF downloads use the current cohort and track selection', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'progress-export');
  await new SignInPage(page).signIn(admin);

  const board = new ProgressBoardPage(page);
  await board.goto();
  await board.cohortSelect.selectOption(seeded.cohortId);
  await board.trackSelect.selectOption(seeded.primaryTrackId);
  await expect(board.row(seeded.youthName)).toBeVisible();

  const [csv] = await Promise.all([page.waitForEvent('download'), board.exportCsv.click()]);
  expect(csv.suggestedFilename()).toMatch(/progress.*\.csv$/);
  const [pdf] = await Promise.all([page.waitForEvent('download'), board.exportPdf.click()]);
  expect(pdf.suggestedFilename()).toMatch(/progress.*\.pdf$/);

  await board.trackSelect.selectOption(seeded.secondTrackId);
  await expect(board.emptyState).toBeVisible();
});
