import { expect, test } from '@playwright/test';
import { SignInPage, YouthQuizAttemptPage, YouthQuizIntroPage, YouthQuizResultPage } from '../pages';
import { seedYouthCurriculum, signInApi } from '../support/api';

// Traces to: L2-LEARN-004, L2-LEARN-005, L2-NFR-009, L2-NFR-010
test('quiz intro, attempt, and result stay contained and keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const youthAuth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const unlock = await request.post(`/api/me/lessons/${seeded.secondLessonId}/complete`, {
    headers: { Authorization: `Bearer ${youthAuth.accessToken}` },
  });
  expect(unlock.ok(), await unlock.text()).toBeTruthy();
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/quizzes/${seeded.quizId}`);

  const intro = new YouthQuizIntroPage(page);
  await expect(intro.heading).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth - innerWidth)).toBeLessThanOrEqual(
    1,
  );
  expect((await intro.startButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await intro.startButton.focus();
  await page.keyboard.press('Enter');

  const attempt = new YouthQuizAttemptPage(page);
  await expect(attempt.questionHeading).toHaveText(seeded.firstQuizQuestion);
  const firstOption = page.getByRole('radio', { name: seeded.firstCorrectAnswer });
  const firstLabel = page.locator('label').filter({ has: firstOption });
  expect((await firstLabel.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await firstOption.focus();
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    page.keyboard.press('Space'),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');
  await attempt.nextButton.focus();
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    page.keyboard.press('Enter'),
  ]);

  const secondOption = page.getByRole('radio', { name: seeded.secondCorrectAnswer });
  await secondOption.focus();
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    page.keyboard.press('Space'),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');
  page.once('dialog', async (dialog) => dialog.accept());
  await attempt.submitButton.focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/youth\/quiz-attempts\/[^/]+\/result$/);
  const attemptId = page.url().split('/').at(-2)!;

  const result = new YouthQuizResultPage(page);
  await expect(page.getByText(/waiting for mentor approval/i)).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth - innerWidth)).toBeLessThanOrEqual(
    1,
  );
  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const approve = await request.post(`/api/mentor/attempts/${attemptId}/approve`, {
    data: {},
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
  });
  expect(approve.ok(), await approve.text()).toBeTruthy();
  await page.reload();

  await expect(page.getByText(seeded.quizBadgeName, { exact: true })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth - innerWidth)).toBeLessThanOrEqual(
    1,
  );
  expect((await result.retakeButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
});
