import { expect, test } from '@playwright/test';
import { MentorQuizReviewPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-004, L2-NFR-009, L2-NFR-010
test('quiz assessment is keyboard operable and contained on mobile', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/reviews/quizzes/${seeded.mentorQuizAttemptId}`);

  const review = new MentorQuizReviewPage(page);
  await expect(review.heading).toBeVisible();
  await review.overrideScore.focus();
  await page.keyboard.type('72');
  expect((await review.approve.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
