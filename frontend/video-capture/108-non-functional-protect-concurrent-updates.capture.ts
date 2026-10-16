import { expect, test } from '@playwright/test';
import { MentorAttendanceCapturePage, SignInPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '108-non-functional-protect-concurrent-updates';
const PAGE = { x: 244, y: 270, width: 1036, height: 400 };

test.setTimeout(120_000);
test('capture: stale save is rejected, nothing is overwritten', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');

  await capture.allPresent.click();
  await capture.note(cast.youthName).fill('Arrived early.');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'editing', PAGE);

  // A second person saves the same session first, so this page now holds a stale version.
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
          note: 'Marked by a second person.',
          expectedVersion: current.roster[0].version,
        },
      ],
    },
  });
  expect(other.ok(), `second save failed: ${other.status()} ${await other.text()}`).toBeTruthy();

  await capture.save.click();
  await expect(page.getByRole('alert')).toContainText(/changed on the server/i);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'conflict', PAGE);
});
