import { expect, test } from '@playwright/test';
import { CohortDetailPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-003, L2-NFR-009, L2-NFR-010
test('cohort detail and membership controls reflow without mobile page overflow', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-mobile');
  await registerUser(request, 'Youth', 'cohort-mobile-available');
  await new SignInPage(page).signIn(admin);

  const detail = new CohortDetailPage(page);
  await detail.goto(seeded.cohortId);
  expect((await detail.editButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await detail.youthSelect.focus();
  await expect(detail.member(seeded.youthName)).toBeVisible();
  await expect.poll(() => detail.horizontalOverflow()).toBeLessThanOrEqual(1);
});
