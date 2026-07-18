import { expect, test } from '@playwright/test';
import { registerUser, seedYouthCurriculum } from '../../support/api';
import { PeopleDirectoryPage, SignInPage } from '../../pages';

// Traces to: L2-ADMIN-001
test('an admin searches the people directory and finds a specific person', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-dir');
  const youth = await registerUser(request, 'Youth', 'dir-target');
  await new SignInPage(page).signIn(admin);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await expect(page).toHaveURL(PeopleDirectoryPage.urlPattern);

  // Narrow by the seeded youth's unique email; their row appears (AC1/AC2).
  await people.search(youth.email);
  await expect(people.rowFor(youth.email)).toBeVisible();
});

// Traces to: L2-ADMIN-001
test('a search matching nobody shows the empty state with an invite CTA', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-empty');
  await new SignInPage(page).signIn(admin);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await people.search(`no-such-person-${crypto.randomUUID()}`);

  await expect(people.emptyState).toBeVisible();
  await expect(people.inviteCta).toBeVisible();
});

// Traces to: L2-ADMIN-001
test('role and cohort filters compose and paging resets when filters change', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-filter');
  await Promise.all(
    Array.from({ length: 12 }, (_, index) => registerUser(request, 'Volunteer', 'page-' + index)),
  );
  await new SignInPage(page).signIn(admin);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await expect(people.pageStatus).toContainText('Page 1 of');
  await expect(people.nextPage).toBeEnabled();
  await people.nextPage.click();
  await expect(people.pageStatus).toContainText('Page 2 of');

  await people.roleFilter('Youth').click();
  await people.cohortFilter.selectOption(seeded.cohortId);
  await expect(people.pageStatus).toContainText('Page 1 of 1');
  await expect(people.rowFor(seeded.youthEmail)).toBeVisible();
  await expect(page.getByTestId('person-row')).toHaveCount(1);
});
