import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { signIn } from '../../support/sign-in';

// Traces to: L2-CONSENT-002
test('a register-created youth is not gated and reaches home', async ({ page, request }) => {
  const creds = await registerYouth(request, 'consent');

  // Discovery: a register-created youth has no guardian on record, so the server
  // treats them as an adult and participation is allowed. This pins the branch
  // that makes the full consent-required flow a Tier-B (needs a real minor).
  const status = await request.get('/api/consent/status', {
    headers: { Authorization: `Bearer ${creds.auth.accessToken}` },
  });
  expect(status.ok()).toBeTruthy();
  expect((await status.json()).participationAllowed).toBe(true);

  // The client renders the allowed outcome: the consent guard permits youth
  // navigation and never diverts to consent-required.
  await signIn(page, creds);
  await expect(page).toHaveURL(/\/youth\/home/);
  await expect(page).not.toHaveURL(/consent-required/);
});
