import { expect, test } from '@playwright/test';
import { registerUser, seedVolunteerSession } from '../../support/api';
import { SignInPage, VolunteerTodayPage } from '../../pages';

// Traces to: L2-VOLUNTEER-001
test('a volunteer signs in, lands on Today, and sees the no-session empty state', async ({
  page,
  request,
}) => {
  const vol = await registerUser(request, 'Volunteer', 'vol-today');
  await new SignInPage(page).signIn(vol);

  // Role-based routing takes a Volunteer to their Today surface.
  await expect(page).toHaveURL(VolunteerTodayPage.urlPattern);

  // A freshly-seeded volunteer has no assigned session today (204 → empty state).
  const today = new VolunteerTodayPage(page);
  await expect(today.noSessionHeading).toBeVisible();
});

test('Today shows the assigned session logistics and links to its attendance roster', async ({
  page,
  request,
}) => {
  const volunteer = await registerUser(request, 'Volunteer', 'vol-today-populated');
  await seedVolunteerSession(request, volunteer.userId);

  await new SignInPage(page).signIn(volunteer);
  const today = new VolunteerTodayPage(page);

  await expect(today.heading).toBeVisible();
  await expect(page.getByText('Main Hall')).toBeVisible();
  await expect(page.getByText('3 on the roster')).toBeVisible();
  await expect(today.takeAttendanceLink).toBeVisible();
});
