import { expect, test } from '@playwright/test';
import { registerUser } from '../../support/api';
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
