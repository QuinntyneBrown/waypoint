import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { MentorAssignmentsPage, SignInPage } from '../e2e/pages';
import { seedYouthCurriculum, SeededUser } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '023-program-administration-assign-mentors';

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

test('capture: assign mentors', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedYouthCurriculum(request);
  const grace = await seedUser(request, 'Mentor', 'Grace Adeyemi');
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const assignments = new MentorAssignmentsPage(page);
  await assignments.goto();
  await assignments.youthSelect.selectOption(seeded.youthId);
  await expect(assignments.mentorSelect).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'overview');

  await assignments.mentorSelect.selectOption(grace.userId);
  await assignments.assignMentor.click();
  await expect(assignments.outcome).toContainText('Mentor assignment saved');
  const card = assignments.mentorLoad(grace.userId);
  await expect(card).toContainText('1 youth');
  await card.scrollIntoViewIfNeeded();
  const box = (await card.boundingBox())!;
  await shot(page, FOLDER, 'load', {
    x: box.x - 4,
    y: box.y - 4,
    width: box.width + 8,
    height: box.height + 8,
  });

  await page.evaluate(() => window.scrollTo(0, 0));
  await assignments.cohortSelect.selectOption(seeded.cohortId);
  await expect(assignments.leadMentorSelect).toHaveValue(seeded.mentorId);
  await assignments.supportingMentor(grace.userId).check();
  await assignments.saveCohortTeam.click();
  await expect(assignments.outcome).toContainText('Cohort mentor team saved');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'team');
});
