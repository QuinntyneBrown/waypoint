import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { PersonDetailPage, SignInPage } from '../e2e/pages';
import { SeededUser, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '030-program-administration-view-person-record';

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

test('capture: view person record', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedYouthCurriculum(request);
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const detail = new PersonDetailPage(page);
  await detail.goto(seeded.youthId);
  await expect(detail.heading).toHaveText(seeded.youthName);
  await expect(detail.panel('Profile')).toContainText(seeded.youthEmail);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'profile');

  for (const [tab, name, text] of [
    ['Guardian & consent', 'guardian', 'Valid'],
    ['Progress', 'progress', '1 of 2 lessons complete'],
    ['Attendance', 'attendance', seeded.progressPresentSessionTitle],
    ['Notes', 'notes', '1:1'],
  ]) {
    await detail.tab(tab).click();
    await expect(detail.panel(tab)).toContainText(text);
    await page.waitForLoadState('networkidle');
    await shot(page, FOLDER, name);
  }
});
