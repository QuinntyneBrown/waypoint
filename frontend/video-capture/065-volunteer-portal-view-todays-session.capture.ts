import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerTodayPage } from '../e2e/pages';
import { registerUser, seedVolunteerSession } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '065-volunteer-portal-view-todays-session';
const PAGE = { x: 362, y: 0, width: 800, height: 600 };

test('capture: view today session', async ({ page, request, browser }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'sam-whitfield');
  await seedVolunteerSession(request, volunteer.userId);

  await new SignInPage(page).signIn(volunteer);
  const today = new VolunteerTodayPage(page);
  await expect(today.heading).toBeVisible();
  await expect(page.getByText('3 on the roster')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'today', PAGE);

  // A volunteer with nothing assigned today sees the empty state.
  const idle = await registerUser(request, 'Volunteer', 'sam-idle');
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(fresh).signIn(idle);
  await expect(new VolunteerTodayPage(fresh).noSessionHeading).toBeVisible();
  await fresh.waitForLoadState('networkidle');
  await shot(fresh, FOLDER, 'empty', PAGE);
});
