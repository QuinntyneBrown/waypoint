import { expect, test } from '@playwright/test';
import { registerYouth, seedYouthCurriculum } from '../../support/api';
import { SessionExpiredPage, SignInPage, YouthHomePage, YouthReflectionPage } from '../../pages';

// Traces to: L2-AUTH-012
test('refresh-token reuse revokes the family and forces the session-expired state', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'reuse');
  await new SignInPage(page).signIn(creds);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);

  // Capture the app's stored refresh token and seed an in-progress draft.
  const session = await page.evaluate(() => sessionStorage.getItem('w.session.v1'));
  const { refreshToken, user } = JSON.parse(session!);
  const draftKey = `w.draft.${user.userId}.reflection`;
  await page.evaluate((k) => localStorage.setItem(k, 'my unsaved reflection'), draftKey);

  // Present the captured refresh token twice: the second use trips reuse
  // detection and revokes the whole token family (L2-AUTH-012 AC2).
  const first = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(first.ok()).toBeTruthy();
  const second = await request.post('/api/auth/refresh', { data: { refreshToken } });
  expect(second.status()).toBe(401);

  // Reload: the app's boot refresh now 401s → forced teardown to session-expired.
  await page.reload();
  const sessionExpired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(sessionExpired.heading).toBeVisible();
  await expect(sessionExpired.signInAgainLink).toBeVisible();

  // The draft survives the forced teardown (L2-PLATFORM-010 AC1 / L2-AUTH-007 AC2).
  const draft = await page.evaluate((k) => localStorage.getItem(k), draftKey);
  expect(draft).toBe('my unsaved reflection');
});

// Traces to: L2-AUTH-007, L2-PLATFORM-010, L2-NFR-007
test('idle expiry revokes the session and restores the owner-bound reflection draft after sign-in', async ({
  page,
  request,
}) => {
  await page.clock.install();
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const reflectionUrl = `/youth/lessons/${seeded.secondLessonId}/reflection`;
  await page.goto(reflectionUrl);

  const reflection = new YouthReflectionPage(page);
  const text = `Idle-safe reflection ${crypto.randomUUID()}`;
  await reflection.reflectionText.fill(text);
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await reflection.optionalResponse.fill('Return here after signing in again.');
  await reflection.shareWithMentor.check();

  await page.clock.fastForward(20 * 60 * 1000);
  await page.keyboard.press('Shift');
  await page.clock.fastForward(20 * 60 * 1000);
  await expect(page).toHaveURL(new RegExp(`${reflectionUrl}$`));

  const activeRefreshToken = await page.evaluate(() => {
    const raw = sessionStorage.getItem('w.session.v1') ?? localStorage.getItem('w.session.v1');
    return raw ? (JSON.parse(raw) as { refreshToken: string }).refreshToken : null;
  });
  expect(activeRefreshToken).not.toBeNull();

  const signOutResponse = page.waitForResponse(
    (response) => response.url().includes('/api/auth/sign-out') && response.status() === 204,
    { timeout: 5_000 },
  );
  await page.clock.fastForward(11 * 60 * 1000);
  await signOutResponse;

  const expired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(expired.heading).toBeVisible();
  expect(
    await page.evaluate(
      () => sessionStorage.getItem('w.session.v1') ?? localStorage.getItem('w.session.v1'),
    ),
  ).toBeNull();
  const capturedDraft = await page.evaluate(() => {
    const raw = sessionStorage.getItem('w.transient-form-draft');
    return raw
      ? (JSON.parse(raw) as {
          ownerUserId: string;
          url: string;
          fields: { key: string; value: string; checked?: boolean }[];
        })
      : null;
  });
  expect(capturedDraft?.ownerUserId).toBe(seeded.youthId);
  expect(capturedDraft?.url).toBe(reflectionUrl);
  expect(capturedDraft?.fields).toEqual(
    expect.arrayContaining([
      expect.objectContaining({ key: 'mood', value: 'thoughtful', checked: true }),
    ]),
  );

  const revoked = await request.post('/api/auth/refresh', {
    data: { refreshToken: activeRefreshToken },
  });
  expect(revoked.status()).toBe(401);

  await expired.signInAgainLink.click();
  await expect(page).toHaveURL(/\/auth\/sign-in\?returnUrl=/);
  await new SignInPage(page).submitCredentials(seeded.youthEmail, seeded.youthPassword);
  await expect(page).toHaveURL(new RegExp(`${reflectionUrl}$`));
  await expect(reflection.reflectionText).toHaveValue(text);
  await expect(page.getByRole('radio', { name: 'Thoughtful' })).toBeChecked();
  await expect(reflection.optionalResponse).toHaveValue('Return here after signing in again.');
  await expect(reflection.shareWithMentor).toBeChecked();
  await expect(page.getByRole('status')).toContainText(/draft restored after session expiry/i);
});
