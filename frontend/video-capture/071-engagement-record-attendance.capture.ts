import { expect, test } from '@playwright/test';
import { MentorAttendanceCapturePage, SignInPage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '071-engagement-record-attendance';
const SHEET = { x: 255, y: 80, width: 1015, height: 580 };

test.setTimeout(120_000);
test('capture: record attendance', async ({ page, request }) => {
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
});
