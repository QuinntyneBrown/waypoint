import { expect, test } from '@playwright/test';
import { ProgressBoardPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-004, L2-NFR-009, L2-NFR-010
test('the progress matrix scrolls inside its container without mobile page overflow', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'progress-mobile');
  await new SignInPage(page).signIn(admin);

  const board = new ProgressBoardPage(page);
  await board.goto();
  await board.cohortSelect.selectOption(seeded.cohortId);
  await board.trackSelect.selectOption(seeded.primaryTrackId);
  await expect(board.row(seeded.youthName)).toBeVisible();
  const dimensions = await board.matrix.evaluate((element) => ({
    clientWidth: element.clientWidth,
    scrollWidth: element.scrollWidth,
  }));
  expect(dimensions.scrollWidth).toBeGreaterThan(dimensions.clientWidth);
  expect((await board.exportCsv.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => board.horizontalOverflow()).toBeLessThanOrEqual(1);
});
