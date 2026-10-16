import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { ProgressBoardPage, SignInPage } from '../e2e/pages';
import { SeededUser, seedYouthCurriculum, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '031-program-administration-view-progress-board';

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

test('capture: view progress board', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedYouthCurriculum(request);
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const board = new ProgressBoardPage(page);
  await board.goto();
  await board.cohortSelect.selectOption(seeded.cohortId);
  await board.trackSelect.selectOption(seeded.primaryTrackId);
  const row = board.row(seeded.youthName);
  await expect(row).toContainText('In progress');
  await expect(board.liveStatus).toContainText('Live');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'board');

  const youthAuth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const complete = await request.post(`/api/me/lessons/${seeded.secondLessonId}/complete`, {
    headers: { Authorization: `Bearer ${youthAuth.accessToken}` },
  });
  expect(complete.ok()).toBeTruthy();
  await expect(row).toContainText('Done');
  await expect(row).toContainText('100%');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'live');

  await board.trackSelect.selectOption(seeded.secondTrackId);
  await expect(board.emptyState).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'empty');
});
