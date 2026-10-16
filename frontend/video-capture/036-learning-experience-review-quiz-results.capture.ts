import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthQuizAttemptPage,
  YouthQuizIntroPage,
  YouthQuizResultPage,
} from '../e2e/pages';
import { seedYouthCurriculum, signInApi } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '036-learning-experience-review-quiz-results';

test('capture: review quiz results', async ({ page, request }) => {
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
  await new YouthQuizIntroPage(page).startButton.click();

  const attempt = new YouthQuizAttemptPage(page);
  const saved = () =>
    page.waitForResponse((r) => r.url().includes('/answers') && r.request().method() === 'PUT');
  await expect(attempt.questionHeading).toHaveText(seeded.firstQuizQuestion);
  await Promise.all([
    saved(),
    page.getByRole('radio', { name: seeded.firstCorrectAnswer }).check(),
  ]);
  await Promise.all([saved(), attempt.nextButton.click()]);
  await expect(attempt.questionHeading).toHaveText(seeded.secondQuizQuestion);
  await Promise.all([
    saved(),
    page.getByRole('radio', { name: seeded.secondCorrectAnswer }).check(),
  ]);
  page.once('dialog', (dialog) => dialog.accept());
  await attempt.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/quiz-attempts\/[^/]+\/result$/);
  const attemptId = page.url().split('/').at(-2)!;

  const result = new YouthQuizResultPage(page);
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
  await expect(page.getByText('Passed', { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.quizBadgeName, { exact: true })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'result');

  await result.retakeButton.scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'retake');
});
