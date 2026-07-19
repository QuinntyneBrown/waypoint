import { expect, test } from '@playwright/test';
import { registerYouth, seedYouthCurriculum, signInApi } from '../../support/api';
import { ConsentRequiredPage, SignInPage, YouthHomePage, YouthLessonPage } from '../../pages';

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
  await new SignInPage(page).signIn(creds);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await expect(page).not.toHaveURL(/consent-required/);
});

// Traces to: L2-CONSENT-002 AC1/AC3/AC5, L2-CONSENT-004 AC1, L2-PLATFORM-011
test('withdrawing consent blocks the next lesson action and fresh consent unblocks the same session', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const lessonUrl = `/youth/lessons/${seeded.secondLessonId}`;
  await page.goto(lessonUrl);
  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);

  const guardian = await signInApi(request, {
    email: seeded.guardianEmail,
    password: seeded.youthPassword,
  });
  const recordResponse = await request.get(`/api/guardian/children/${seeded.youthId}/consent`, {
    headers: { Authorization: `Bearer ${guardian.accessToken}` },
  });
  expect(recordResponse.ok(), await recordResponse.text()).toBeTruthy();
  const records = (await recordResponse.json()) as { consentId: string }[];
  const withdrawal = await request.post(`/api/consent/${records[0].consentId}/withdraw`, {
    headers: { Authorization: `Bearer ${guardian.accessToken}` },
  });
  expect(withdrawal.ok(), await withdrawal.text()).toBeTruthy();

  page.once('dialog', async (dialog) => dialog.accept());
  await lesson.completeButton.click();
  await expect(page).toHaveURL(/\/consent-required$/);
  await expect(new ConsentRequiredPage(page).heading).toBeVisible();

  const recaptured = await request.post('/api/consent', {
    headers: { Authorization: `Bearer ${guardian.accessToken}` },
    data: {
      youthUserId: seeded.youthId,
      guardianName: seeded.guardianName,
      relationship: 'Parent',
      participationConsent: true,
      mediaConsent: false,
      mediaAllPhotosVideo: false,
      mediaGroupOnly: false,
      mediaFirstNameCaptions: false,
      signature: seeded.guardianName,
      term: null,
    },
  });
  expect(recaptured.ok(), await recaptured.text()).toBeTruthy();

  await page.goto(lessonUrl);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  page.once('dialog', async (dialog) => dialog.accept());
  await lesson.completeButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}/reflection$`));
});
