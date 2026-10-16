import { expect, test } from '@playwright/test';
import { ConsentRequiredPage, SignInPage, YouthLessonPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedGuardianCast, shotElement } from './guardian-cast';
import { rename, seedCast } from './mentoring-cast';
import { shot } from './shots';

const FOLDER = '076-consent-and-safeguarding-enforce-minor-safeguards';

test('capture: enforce minor safeguards', async ({ page, request, browser }) => {
  test.setTimeout(180_000);
  const seeded = await seedCast(request, 'Imani Clarke');
  await rename(
    request,
    { email: seeded.guardianEmail, password: seeded.youthPassword },
    'Ruth Clarke',
  );
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.goto(`/youth/lessons/${seeded.secondLessonId}`);
  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'lesson', { x: 244, y: 56, width: 1036, height: 500 });

  const guardian = await signInApi(request, {
    email: seeded.guardianEmail,
    password: seeded.youthPassword,
  });
  const records = (await (
    await request.get(`/api/guardian/children/${seeded.youthId}/consent`, {
      headers: { Authorization: `Bearer ${guardian.accessToken}` },
    })
  ).json()) as { consentId: string }[];
  const withdrawn = await request.post(`/api/consent/${records[0].consentId}/withdraw`, {
    headers: { Authorization: `Bearer ${guardian.accessToken}` },
  });
  expect(withdrawn.ok()).toBeTruthy();

  page.once('dialog', (dialog) => dialog.accept());
  await lesson.completeButton.click();
  await expect(page).toHaveURL(/\/consent-required$/);
  await expect(new ConsentRequiredPage(page).heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'gate', { x: 244, y: 56, width: 1036, height: 600 });

  // A guardian sees only what a mentor chose to share; the private check-in never appears.
  const cast = await seedGuardianCast(request, 'Imani Clarke');
  const other = await (await browser.newContext({ baseURL: 'http://localhost:4200' })).newPage();
  await new SignInPage(other).signIn(cast.guardian);
  await other.goto('/guardian/progress');
  const shared = other.getByTestId('guardian-shared-checkins');
  await expect(shared).toContainText(cast.family.sharedActionItems);
  await expect(other.getByText(cast.family.privateSecret)).toHaveCount(0);
  await other.waitForLoadState('networkidle');
  await shotElement(other, FOLDER, 'shared-only', shared);
});
