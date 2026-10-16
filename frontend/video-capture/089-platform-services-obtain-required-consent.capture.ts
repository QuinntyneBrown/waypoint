import { expect, test } from '@playwright/test';
import { ConsentRequiredPage, SignInPage, YouthLessonPage } from '../e2e/pages';
import { seedYouthCurriculum, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '089-platform-services-obtain-required-consent';
const STATE = { x: 300, y: 130, width: 680, height: 500 };

test('capture: consent required', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const lessonUrl = `/youth/lessons/${seeded.secondLessonId}`;
  await page.goto(lessonUrl);
  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);

  // The guardian withdraws consent, so the youth's next action is gated.
  const guardian = await signInApi(request, {
    email: seeded.guardianEmail,
    password: seeded.youthPassword,
  });
  const headers = { Authorization: `Bearer ${guardian.accessToken}` };
  const records = (await (
    await request.get(`/api/guardian/children/${seeded.youthId}/consent`, { headers })
  ).json()) as { consentId: string }[];
  const withdrawn = await request.post(`/api/consent/${records[0].consentId}/withdraw`, {
    headers,
  });
  expect(withdrawn.ok()).toBeTruthy();

  page.once('dialog', async (dialog) => dialog.accept());
  await lesson.completeButton.click();
  const consent = new ConsentRequiredPage(page);
  await expect(page).toHaveURL(/\/consent-required$/);
  await expect(consent.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'consent-required', STATE);

  await consent.notifyGuardianButton.click();
  await expect(consent.deliveryConfirmation).toBeVisible();
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'notified', STATE);
});
