import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { CohortDetailPage, CohortsPage, SignInPage } from '../e2e/pages';
import { SeededUser, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '027-program-administration-manage-cohorts';

async function seedUser(
  request: APIRequestContext,
  role: string,
  displayName: string,
): Promise<SeededUser> {
  const email = `${displayName.split(' ')[0].toLowerCase()}-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok()).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, displayName, role, userId };
}

/** Crop a screenshot to an element, with a little padding. */
async function shotOf(page: Page, name: string, target: Locator, pad = 12): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  const box = (await target.boundingBox())!;
  await shot(page, FOLDER, name, {
    x: Math.max(0, box.x - pad),
    y: Math.max(0, box.y - pad),
    width: Math.min(1280, box.width + pad * 2),
    height: Math.min(800, box.height + pad * 2),
  });
}

test('capture: manage cohorts', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedYouthCurriculum(request);
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  const amara = await seedUser(request, 'Youth', 'Amara Bell');
  await new SignInPage(page).signIn(admin);

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  await expect(cohorts.createButton).toBeVisible();
  await cohorts.createButton.click();
  const dialog = cohorts.createDialog;
  const cohortName = `Fall Cohort ${crypto.randomUUID().slice(0, 4)}`;
  await dialog.getByLabel('Cohort name').fill(cohortName);
  await dialog.getByLabel('Curriculum track').selectOption(seeded.primaryTrackId);
  await dialog.getByLabel('Lead mentor').selectOption(seeded.mentorId);
  await dialog.getByLabel('Start date').fill('2026-09-01');
  await dialog.getByLabel('End date').fill('2027-01-15');
  await dialog.getByLabel('Meeting location').fill('Waypoint Community Hall');
  await shotOf(page, 'create-dialog', dialog, 20);

  await dialog.getByRole('button', { name: 'Create cohort' }).click();
  const card = cohorts.card(cohortName);
  await expect(card).toContainText('Waypoint Community Hall');
  await shotOf(page, 'card', card, 16);

  await card.getByRole('link', { name: cohortName }).click();
  const detail = new CohortDetailPage(page);
  await expect(detail.heading).toHaveText(cohortName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'detail');

  await detail.youthSelect.selectOption(amara.userId);
  await detail.addYouth.click();
  await expect(detail.member(amara.displayName)).toBeVisible();
  await expect(detail.memberCount).toContainText('1 member');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'members');

  await detail.editButton.click();
  await expect(detail.editDialog).toBeVisible();
  await page.waitForTimeout(600);
  await shotOf(page, 'edit-dialog', detail.editDialog, 20);
});
