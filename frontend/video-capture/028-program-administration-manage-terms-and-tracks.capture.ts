import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { OrganizationSettingsPage, SignInPage } from '../e2e/pages';
import { SeededUser, seedYouthCurriculum } from '../e2e/support/api';
import { join } from 'node:path';
import { shot } from './shots';

const VIDEOS = join(__dirname, '..', '..', 'docs', 'videos');
const FOLDER = '028-program-administration-manage-terms-and-tracks';

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

/** Screenshot an element at the current (narrow) viewport. */
async function shotNarrow(page: Page, name: string, target: Locator): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  await target.screenshot({ path: join(VIDEOS, FOLDER, 'images', `${name}-1280.png`) });
}

test('capture: manage terms and tracks', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedYouthCurriculum(request);
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const settings = new OrganizationSettingsPage(page);
  await settings.goto();
  await expect(settings.termName).toBeVisible();
  const termName = `Fall ${crypto.randomUUID().slice(0, 4)}`;
  await settings.termName.fill(termName);
  await settings.termStart.fill('2026-09-01');
  await settings.termEnd.fill('2026-12-15');
  await settings.termStatus.selectOption('Upcoming');
  await shotOf(page, 'term-form', page.locator('section[aria-labelledby="terms-heading"]'));

  await settings.saveTerm.click();
  await expect(settings.term(termName)).toContainText('Upcoming');
  await shotOf(page, 'term-saved', page.locator('section[aria-labelledby="terms-heading"]'));

  // The shared test database holds many tracks; show only this one, in a narrow window.
  const track = settings.track(seeded.primaryTrackId);
  await expect(track).toContainText('Enabled');
  await page.evaluate((id) => {
    document
      .querySelectorAll('[data-testid="track-row"]')
      .forEach((row) => row.getAttribute('data-track-id') !== id && row.remove());
  }, seeded.primaryTrackId);
  await page.setViewportSize({ width: 820, height: 800 });
  const tracks = page.locator('section[aria-labelledby="tracks-heading"]');
  await shotNarrow(page, 'track-enabled', tracks);
  await track.getByRole('button', { name: 'Pause track' }).click();
  await expect(track).toContainText('Paused');
  await shotNarrow(page, 'track-paused', tracks);
});
