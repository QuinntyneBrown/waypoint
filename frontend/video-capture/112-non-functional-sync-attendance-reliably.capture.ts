import { expect, test } from '@playwright/test';
import {
  MentorAttendanceCapturePage,
  SignInPage,
  VolunteerAttendancePage,
  VolunteerTodayPage,
} from '../e2e/pages';
import { seedVolunteerSession, signInApi } from '../e2e/support/api';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '112-non-functional-sync-attendance-reliably';
const TOP = { x: 0, y: 0, width: 1280, height: 440 };
const PAGE = { x: 244, y: 92, width: 1036, height: 560 };
const WIDE = { x: 244, y: 270, width: 1036, height: 530 };

test.setTimeout(240_000);

test('capture: volunteer attendance queued offline then synced once', async ({
  page,
  request,
  context,
}) => {
  const email = `sam-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const created = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Sam Whitfield', role: 'Volunteer' },
  });
  expect(created.ok()).toBeTruthy();
  const { userId } = (await created.json()) as { userId: string };
  const seeded = await seedVolunteerSession(request, userId);

  await signInSlow(page, { email, password });
  await new VolunteerTodayPage(page).takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(seeded.youth.length);
  await attendance.allPresentButton.click();
  await page.waitForLoadState('networkidle');

  const banner = page.locator('.w-offline');
  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await expect(banner).toHaveClass(/is-visible/);
  await page.waitForTimeout(400);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'offline-banner', TOP);

  await attendance.markAbsent(seeded.youth[0].name);
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/saved offline.*waiting to sync/i);
  await expect(page.getByText(/pending sync/i)).toBeVisible();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'queued', PAGE);

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(banner).not.toHaveClass(/is-visible/);
  await expect(attendance.syncStatus).toContainText(/all attendance changes synced/i);
  await page.waitForTimeout(400);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'synced', PAGE);
});

test('capture: a queued change that conflicts is shown for review', async ({
  page,
  request,
  context,
}) => {
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');

  await context.setOffline(true);
  await page.evaluate(() => window.dispatchEvent(new Event('offline')));
  await capture.allPresent.click();
  await capture.save.click();
  await expect(capture.status).toContainText(/saved offline.*1.*waiting to sync/i);

  const mentor = await signInApi(request, {
    email: cast.mentorEmail,
    password: cast.mentorPassword,
  });
  const headers = { Authorization: `Bearer ${mentor.accessToken}` };
  const roster = await request.get(`/api/attendance/${cast.scheduleSessionId}/roster`, { headers });
  const current = (await roster.json()) as {
    roster: { youthUserId: string; version: string | null }[];
  };
  const other = await request.post('/api/attendance', {
    headers: { ...headers, 'Idempotency-Key': crypto.randomUUID() },
    data: {
      sessionId: cast.scheduleSessionId,
      marks: [
        {
          youthId: current.roster[0].youthUserId,
          status: 'Absent',
          note: null,
          expectedVersion: current.roster[0].version,
        },
      ],
    },
  });
  expect(other.ok(), `server edit failed: ${other.status()} ${await other.text()}`).toBeTruthy();

  await context.setOffline(false);
  await page.evaluate(() => window.dispatchEvent(new Event('online')));
  await expect(page.getByText(/choose which attendance to keep/i)).toBeVisible();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'conflict', WIDE);
});
