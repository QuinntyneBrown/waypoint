import { expect, test } from '@playwright/test';
import { CohortDetailPage, CohortsPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ADMIN-003
test('an admin creates a fully described cohort from authoritative options', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-create');
  const cohortName = `E2E Cohort ${crypto.randomUUID().slice(0, 8)}`;
  await new SignInPage(page).signIn(admin);

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  await cohorts.createButton.click();
  const dialog = cohorts.createDialog;
  await dialog.getByLabel('Cohort name').fill(cohortName);
  await dialog.getByLabel('Curriculum track').selectOption(seeded.primaryTrackId);
  await dialog.getByLabel('Lead mentor').selectOption(seeded.mentorId);
  await dialog.getByLabel('Start date').fill('2026-09-01');
  await dialog.getByLabel('End date').fill('2027-01-15');
  await dialog.getByLabel('Meeting location').fill('Waypoint Community Hall');
  await dialog.getByRole('button', { name: 'Create cohort' }).click();

  const card = cohorts.card(cohortName);
  await expect(card).toContainText(seeded.primaryTrackName);
  await expect(card).toContainText(seeded.mentorName);
  await expect(card).toContainText('Waypoint Community Hall');
  await card.getByRole('link', { name: cohortName }).click();
  await expect(new CohortDetailPage(page).heading).toHaveText(cohortName);
});

// Traces to: L2-ADMIN-003
test('cohort detail supports audited edits, existing-youth membership, and an invite path', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-detail');
  const availableYouth = await registerUser(request, 'Youth', 'cohort-member');
  await new SignInPage(page).signIn(admin);

  const detail = new CohortDetailPage(page);
  await detail.goto(seeded.cohortId);
  await expect(detail.heading).toHaveText(seeded.cohortName);
  await expect(detail.member(seeded.youthName)).toBeVisible();
  await expect(detail.inviteYouth).toHaveAttribute('href', '/admin/invitations');

  await detail.youthSelect.selectOption(availableYouth.userId);
  await detail.addYouth.click();
  await expect(detail.member(availableYouth.displayName)).toBeVisible();
  await expect(detail.memberCount).toContainText('2 members');

  await detail.editButton.click();
  const dialog = detail.editDialog;
  const updatedName = `${seeded.cohortName} Updated`;
  await dialog.getByLabel('Cohort name').fill(updatedName);
  await dialog.getByLabel('Meeting location').fill('Updated community room');
  await dialog.getByRole('button', { name: 'Save changes' }).click();
  await expect(detail.heading).toHaveText(updatedName);
  await expect(page.getByText('Updated community room', { exact: false })).toBeVisible();
});

// Traces to: L2-ADMIN-003 AC4
test('a failed cohort list can be retried without losing the page', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-retry');
  let allowList = false;
  await page.route('**/api/cohorts', async (route) => {
    if (!allowList) {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: '{"title":"Unavailable"}',
      });
      return;
    }
    await route.continue();
  });
  await new SignInPage(page).signIn(admin);

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  await expect(page.getByRole('alert')).toContainText('Unavailable');
  allowList = true;
  await cohorts.retry.click();
  await expect(cohorts.heading).toBeVisible();
  await expect(page.getByRole('alert')).toHaveCount(0);
});

// Traces to: L2-ADMIN-003 AC4
test('an empty cohort list offers the first creation action', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-empty');
  await page.route('**/api/cohorts', async (route) => {
    await route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
  });
  await new SignInPage(page).signIn(admin);

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  await expect(cohorts.emptyState).toBeVisible();
  await expect(page.getByRole('button', { name: 'New cohort' }).first()).toBeVisible();
});
