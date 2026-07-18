import { expect, test } from '@playwright/test';
import { MentorSubmissionReviewPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-003, L2-NFR-009, L2-NFR-010
test('rubric grading remains contained and keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/reviews/submissions/${seeded.mentorReviewSubmissionId}`);
  const review = new MentorSubmissionReviewPage(page);
  await review.score(seeded.mentorReviewCriterion).focus();
  await page.keyboard.type('8');
  await review.comment.focus();
  await page.keyboard.type('Saved from a phone-sized viewport.');
  await review.saveDraft.focus();
  await page.keyboard.press('Enter');
  await expect(review.status).toContainText('Draft saved');
  expect((await review.saveDraft.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
