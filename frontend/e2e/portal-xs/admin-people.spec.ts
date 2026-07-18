import { expect, test } from '@playwright/test';
import { PeopleDirectoryPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-001, L2-NFR-009, L2-NFR-010
test('people filters reflow without horizontal overflow on mobile', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'mobile-people-filter');
  await new SignInPage(page).signIn(admin);
  const people = new PeopleDirectoryPage(page);
  await people.goto();

  await people.roleFilter('Youth').click();
  await people.cohortFilter.selectOption(seeded.cohortId);
  await expect(people.rowFor(seeded.youthEmail)).toBeVisible();
  expect((await people.roleFilter('Youth').boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(await people.horizontalOverflow()).toBeLessThanOrEqual(1);
});
