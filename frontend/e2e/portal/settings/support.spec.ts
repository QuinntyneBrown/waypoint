import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-PLATFORM-005
test('a support request is submitted and acknowledged', async ({ page, request }) => {
  const creds = await registerYouth(request, 'support');
  await signIn(page, creds);
  await page.goto('/support');

  await page.getByLabel(/how can we help/i).fill('I can’t find my lessons anywhere.');
  // Category defaults to the first option; reply-to email is prefilled from the session.
  await page.getByRole('button', { name: /send message/i }).click();

  await expect(page.getByRole('heading', { name: /got your message/i })).toBeVisible();
});
