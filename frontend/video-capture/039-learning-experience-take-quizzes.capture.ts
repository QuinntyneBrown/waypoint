import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthQuizAttemptPage,
  YouthQuizIntroPage,
  YouthQuizResultPage,
} from '../e2e/pages';
import { seedYouthCurriculum, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '039-learning-experience-take-quizzes';

test('capture: take quiz', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const youthAuth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const unlock = await request.post(`/api/me/lessons/${seeded.secondLessonId}/complete`, {
    headers: { Authorization: `Bearer ${youthAuth.accessToken}` },
  });
  expect(unlock.ok(), await unlock.text()).toBeTruthy();
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto(`/youth/quizzes/${seeded.quizId}`);

  const intro = new YouthQuizIntroPage(page);
  await expect(intro.heading).toHaveText(seeded.quizTitle);
  await expect(page.getByText('Attempt 1 of 2', { exact: true })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'intro');
  await intro.startButton.click();

  const attempt = new YouthQuizAttemptPage(page);
  await expect(attempt.questionHeading).toHaveText(seeded.firstQuizQuestion);
  await Promise.all([
    page.waitForResponse((r) => r.url().includes('/answers') && r.request().method() === 'PUT'),
    page.getByRole('radio', { name: seeded.firstCorrectAnswer }).check(),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');
  await shot(page, FOLDER, 'question');

  await Promise.all([
    page.waitForResponse((r) => r.url().includes('/answers') && r.request().method() === 'PUT'),
    attempt.nextButton.click(),
  ]);
  await expect(attempt.questionHeading).toHaveText(seeded.secondQuizQuestion);
  await Promise.all([
    page.waitForResponse((r) => r.url().includes('/answers') && r.request().method() === 'PUT'),
    page.getByRole('radio', { name: seeded.secondCorrectAnswer }).check(),
  ]);
  await expect(page.getByRole('status')).toHaveText('Answer saved.');
  await page.reload();
  await expect(attempt.progress).toContainText('Question 2 of 2');
  await expect(page.getByRole('radio', { name: seeded.secondCorrectAnswer })).toBeChecked();
  await shot(page, FOLDER, 'resumed');

  page.once('dialog', async (dialog) => dialog.accept());
  await attempt.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/quiz-attempts\/[^/]+\/result$/);
  const attemptId = page.url().split('/').at(-2)!;
  await expect(page.getByText(/waiting for mentor approval/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'waiting');

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
  const result = new YouthQuizResultPage(page);
  await expect(page.getByText('Passed', { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.quizBadgeName, { exact: true })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'result');
  await expect(result.retakeButton).toBeVisible();
});
