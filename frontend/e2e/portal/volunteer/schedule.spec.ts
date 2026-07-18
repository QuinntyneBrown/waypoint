import { expect, test } from '@playwright/test';
import { registerUser } from '../../support/api';
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
