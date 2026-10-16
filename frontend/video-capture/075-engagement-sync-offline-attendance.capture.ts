import { expect, test } from '@playwright/test';
import { MentorAttendanceCapturePage, SignInPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedEngagementCast, seedPat } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '075-engagement-sync-offline-attendance';
const BANNER = { x: 0, y: 56, width: 1280, height: 620 };
const SHEET = { x: 255, y: 80, width: 1015, height: 580 };

test.setTimeout(120_000);
test('capture: sync offline attendance', async ({ page, context, request }) => {
  const cast = await seedEngagementCast(request);
  await new SignInPage(page).signIn({ email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');

  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await capture.allPresent.click();
  await capture.note(cast.youthName).fill('Queued while offline.');
  await shot(page, FOLDER, 'offline-marked', BANNER);
  await capture.save.click();
  await expect(capture.status).toContainText(/saved offline.*1.*waiting to sync/i);
  await shot(page, FOLDER, 'queued', BANNER);

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(capture.status).toContainText(/all attendance changes synced/i);
  await shot(page, FOLDER, 'synced', SHEET);
});

test('capture: sync conflict', async ({ page, context, request }) => {
  const cast = await seedEngagementCast(request);
  const pat = await signInApi(request, await seedPat(request));
  await new SignInPage(page).signIn({ email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');

  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await capture.allPresent.click();
  await capture.save.click();
  await expect(capture.status).toContainText(/waiting to sync/i);

  const directorSave = await request.post('/api/attendance', {
    headers: { Authorization: `Bearer ${pat.accessToken}`, 'Idempotency-Key': crypto.randomUUID() },
    data: {
      sessionId: cast.scheduleSessionId,
      marks: [{ youthId: cast.youthId, status: 'Absent', note: null }],
    },
  });
  expect(directorSave.ok()).toBeTruthy();

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(capture.status).toContainText(/conflicts with a newer server edit/i);
  await shot(page, FOLDER, 'conflict', { x: 255, y: 300, width: 1015, height: 500 });
  await page.getByRole('button', { name: /apply my changes/i }).click();
  await expect(capture.status).toContainText(/all attendance changes synced/i);
});
