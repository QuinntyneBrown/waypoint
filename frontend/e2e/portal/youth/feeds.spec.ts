import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, YouthAnnouncementsPage, YouthNotificationsPage } from '../../pages';

// Traces to: L2-ENGAGE-006
test('the youth announcements feed shows an empty state for a new youth', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'annfeed');
  await new SignInPage(page).signIn(creds);

  const announcements = new YouthAnnouncementsPage(page);
  await announcements.goto();
  await expect(announcements.emptyStateHeading).toBeVisible();
});

// Traces to: L2-ENGAGE-008
test('the youth notifications feed shows an empty state for a new youth', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'notiffeed');
  await new SignInPage(page).signIn(creds);

  const notifications = new YouthNotificationsPage(page);
  await notifications.goto();
  await expect(notifications.emptyStateHeading).toBeVisible();
});
