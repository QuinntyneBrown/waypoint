import { expect, test } from '@playwright/test';
import { SignInPage, VolunteerSchedulePage } from '../e2e/pages';
import { registerUser, seedVolunteerSession } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '066-volunteer-portal-view-upcoming-schedule';
const PAGE = { x: 362, y: 0, width: 800, height: 640 };

test('capture: view upcoming schedule', async ({ page, request, browser }) => {
  const volunteer = await registerUser(request, 'Volunteer', 'sam-whitfield');
  await seedVolunteerSession(request, volunteer.userId, 2);
  await seedVolunteerSession(request, volunteer.userId, 5);
  await seedVolunteerSession(request, volunteer.userId, 9);

  await new SignInPage(page).signIn(volunteer);
  const schedule = new VolunteerSchedulePage(page);
  await schedule.goto();
  await expect(page.getByText('Main Hall').first()).toBeVisible();
  await expect(page.getByText('upcoming', { exact: true }).first()).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'schedule', PAGE);

  const idle = await registerUser(request, 'Volunteer', 'sam-idle');
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(fresh).signIn(idle);
  const empty = new VolunteerSchedulePage(fresh);
  await empty.goto();
  await expect(empty.noSessionsHeading).toBeVisible();
  await fresh.waitForLoadState('networkidle');
  await shot(fresh, FOLDER, 'empty', PAGE);
});
