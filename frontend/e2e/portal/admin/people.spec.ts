import { expect, test } from '@playwright/test';
import { registerUser } from '../../support/api';
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
