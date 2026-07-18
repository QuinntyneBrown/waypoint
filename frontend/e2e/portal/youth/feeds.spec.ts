import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-ENGAGE-006
test('the youth announcements feed shows an empty state for a new youth', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'annfeed');
  await signIn(page, creds);
  await page.goto('/youth/announcements');
  await expect(page.getByRole('heading', { name: /no announcements yet/i })).toBeVisible();
});

// Traces to: L2-ENGAGE-008
test('the youth notifications feed shows an empty state for a new youth', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'notiffeed');
  await signIn(page, creds);
  await page.goto('/youth/notifications');
  await expect(page.getByRole('heading', { name: /all caught up/i })).toBeVisible();
});
