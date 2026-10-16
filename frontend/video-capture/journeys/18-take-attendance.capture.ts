import { expect, test } from '@playwright/test';
import { MentorAttendanceCapturePage, SignInPage } from '../../e2e/pages';
import { seedEngagementCast } from '../engagement-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/18-take-attendance';
const BANNER = { x: 0, y: 0, width: 1280, height: 640 };
const SHEET = { x: 255, y: 80, width: 1015, height: 580 };

test.setTimeout(120_000);
test('capture: take attendance journey', async ({ page, context, request }) => {
  const cast = await seedEngagementCast(request);
  await new SignInPage(page).signIn({ email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'roster', SHEET);

  await capture.statusButton(cast.youthName, 'Late').click();
  await capture.note(cast.youthName).fill('Transit delay; joined ready to participate.');
  await shot(page, FOLDER, 'marked', SHEET);

  await capture.save.click();
  await expect(capture.status).toContainText(/attendance saved/i);
  await shot(page, FOLDER, 'saved', SHEET);

  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await capture.allPresent.click();
  await capture.note(cast.youthName).fill('Queued while offline.');
  await capture.save.click();
  await expect(capture.status).toContainText(/saved offline.*1.*waiting to sync/i);
  await shot(page, FOLDER, 'queued', BANNER);

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(capture.status).toContainText(/all attendance changes synced/i);
  await shot(page, FOLDER, 'synced', SHEET);
});
