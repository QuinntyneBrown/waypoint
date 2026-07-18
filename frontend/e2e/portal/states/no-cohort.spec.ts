import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-PLATFORM-011
test('an unplaced youth sees the no-cohort state on home with a message-mentor path', async ({
  page,
  request,
}) => {
  // Register-created youth are never placed in a cohort.
  const creds = await registerYouth(request, 'nocohort');
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/youth\/home/);

  await expect(page.getByRole('heading', { name: /not in a cohort yet/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /message your mentor/i })).toBeVisible();
});
