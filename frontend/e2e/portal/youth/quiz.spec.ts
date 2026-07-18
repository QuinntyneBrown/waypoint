import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthQuizAttemptPage,
  YouthQuizIntroPage,
  YouthQuizResultPage,
} from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-LEARN-004, L2-LEARN-005
test('youth resumes an autosaved quiz, submits immutably, and sees approved review and badge', async ({
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
  await expect(intro.heading).toHaveText(seeded.quizTitle);
  await expect(page.getByText('2 questions', { exact: true })).toBeVisible();
  await expect(page.getByText('4 minutes', { exact: true })).toBeVisible();
  await expect(page.getByText('Attempt 1 of 2', { exact: true })).toBeVisible();
  await expect(page.getByText('70% pass mark', { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.quizBadgeName, { exact: true })).toBeVisible();
  await intro.startButton.click();

  const attempt = new YouthQuizAttemptPage(page);
  await expect(attempt.questionHeading).toHaveText(seeded.firstQuizQuestion);
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    page.getByRole('radio', { name: seeded.firstCorrectAnswer }).check(),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    attempt.nextButton.click(),
  ]);
  await expect(attempt.questionHeading).toHaveText(seeded.secondQuizQuestion);
  await Promise.all([
    page.waitForResponse(
      (response) => response.url().includes('/answers') && response.request().method() === 'PUT',
    ),
    page.getByRole('radio', { name: seeded.secondCorrectAnswer }).check(),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');

  await page.reload();
  await expect(attempt.progress).toContainText('Question 2 of 2');
  await expect(attempt.questionHeading).toHaveText(seeded.secondQuizQuestion);
  await expect(page.getByRole('radio', { name: seeded.secondCorrectAnswer })).toBeChecked();

  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toMatch(/cannot be changed/i);
    await dialog.accept();
  });
  await attempt.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/quiz-attempts\/[^/]+\/result$/);
  const attemptId = page.url().split('/').at(-2)!;

  const result = new YouthQuizResultPage(page);
  await expect(result.heading).toContainText(/quiz result/i);
  await expect(page.getByText(/waiting for mentor approval/i)).toBeVisible();
  await expect(page.getByText(/\d+%/)).toHaveCount(0);

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
  await expect(page.getByText('100%', { exact: true })).toBeVisible();
  await expect(page.getByText('Passed', { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.quizBadgeName, { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.firstQuizQuestion, { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.secondQuizQuestion, { exact: true })).toBeVisible();

  await result.retakeButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/quizzes/${seeded.quizId}/attempt$`));
  await expect(page.getByText('Attempt 2 of 2', { exact: true })).toBeVisible();
});
