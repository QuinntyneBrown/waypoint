import { expect, test } from '@playwright/test';
import {
  SignInPage,
  VolunteerAttendancePage,
  VolunteerSchedulePage,
  VolunteerTodayPage,
} from '../../e2e/pages';
import { seedVolunteerSession } from '../../e2e/support/api';
import { shot } from '../shots';

const FOLDER = 'journeys/30-volunteer-portal';
const PAGE = { x: 362, y: 0, width: 800, height: 660 };

test('capture: volunteer portal journey', async ({ page, request, browser }) => {
  test.setTimeout(240000);
  const sam = {
    email: `sam-${crypto.randomUUID()}@waypoint.demo`,
    password: 'Password123',
  };
  const seededUser = await request.post('/api/_test/users', {
    data: { ...sam, displayName: 'Sam Whitfield', role: 'Volunteer' },
  });
  expect(seededUser.ok()).toBeTruthy();
  const { userId } = (await seededUser.json()) as { userId: string };
  const today = await seedVolunteerSession(request, userId, 0);
  await seedVolunteerSession(request, userId, 3);
  await seedVolunteerSession(request, userId, 7);

  await new SignInPage(page).signIn(sam);
  const home = new VolunteerTodayPage(page);
  await expect(home.heading).toBeVisible();
  await expect(page.getByText('3 on the roster')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'today', { x: 362, y: 0, width: 800, height: 600 });

  await home.takeAttendanceLink.click();
  const attendance = new VolunteerAttendancePage(page);
  await expect(attendance.rosterRows).toHaveCount(today.youth.length);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'roster', PAGE);

  await attendance.allPresentButton.click();
  await attendance.markAbsent(today.youth[1].name);
  await attendance.saveButton.click();
  await expect(attendance.syncStatus).toContainText(/attendance saved/i);
  await shot(page, FOLDER, 'saved', PAGE);

  const schedule = new VolunteerSchedulePage(page);
  await schedule.goto();
  await expect(page.getByText('Main Hall').first()).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'schedule', { x: 362, y: 0, width: 800, height: 700 });

  // A volunteer with nothing assigned today sees a calm empty state.
  const idle = {
    email: `idle-${crypto.randomUUID()}@waypoint.demo`,
    password: 'Password123',
  };
  const idleUser = await request.post('/api/_test/users', {
    data: { ...idle, displayName: 'Sam Whitfield', role: 'Volunteer' },
  });
  expect(idleUser.ok()).toBeTruthy();
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(fresh).signIn(idle);
  await expect(new VolunteerTodayPage(fresh).noSessionHeading).toBeVisible();
  await fresh.waitForLoadState('networkidle');
  await shot(fresh, FOLDER, 'empty', { x: 362, y: 0, width: 800, height: 600 });
});
