import { expect, test } from '@playwright/test';
import { registerUser, seedVolunteerSession } from '../../support/api';
import { SignInPage, VolunteerSchedulePage } from '../../pages';

// Traces to: L2-VOLUNTEER-002
test('a volunteer with no upcoming sessions sees the schedule empty state', async ({
  page,
  request,
}) => {
  const vol = await registerUser(request, 'Volunteer', 'vol-sched');
  await new SignInPage(page).signIn(vol);

  const schedule = new VolunteerSchedulePage(page);
  await schedule.goto();
  await expect(page).toHaveURL(VolunteerSchedulePage.urlPattern);
  await expect(schedule.noSessionsHeading).toBeVisible();
});

test('the schedule lists an upcoming assigned session and its status', async ({
  page,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-sched-populated');
  await seedVolunteerSession(request, volunteer.userId, 2);
  await new SignInPage(page).signIn(volunteer);

  const schedule = new VolunteerSchedulePage(page);
  await schedule.goto();

  await expect(page.getByText(/Community Workshop/)).toBeVisible();
  await expect(page.getByText('Main Hall')).toBeVisible();
  await expect(page.getByText('upcoming', { exact: true })).toBeVisible();
});
