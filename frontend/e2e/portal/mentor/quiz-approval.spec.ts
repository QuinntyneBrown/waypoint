import { expect, test } from '@playwright/test';
import { MentorQuizReviewPage, MentorReviewsPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-004, L2-LEARN-005
test('mentor reviews answers, overrides the auto-score, and sees the approval record', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/reviews');

  const queue = new MentorReviewsPage(page);
  const quizItem = queue.items.filter({
    has: page.locator(`a[href$="${seeded.mentorQuizAttemptId}"]`),
  });
  await expect(quizItem).toContainText(seeded.youthName);
  await expect(quizItem).toContainText(seeded.mentorQuizTitle);
  await quizItem.getByRole('link', { name: /review/i }).click();

  const review = new MentorQuizReviewPage(page);
  await expect(review.heading).toHaveText(seeded.mentorQuizTitle);
  await expect(review.autoScore).toHaveText(`${seeded.mentorQuizAutoScore}%`);
  await expect(page.getByText(seeded.mentorQuizQuestion, { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.mentorQuizSelectedAnswer, { exact: true })).toBeVisible();
  await review.overrideScore.fill('101');
  await review.approve.click();
  await expect(page.getByRole('alert')).toContainText('between 0 and 100');
  await review.overrideScore.fill('75');
  page.once('dialog', (dialog) => dialog.accept());
  await review.approve.click();

  await expect(review.auditRecord).toContainText('Auto-scored 40%');
  await expect(review.auditRecord).toContainText('Approved 75%');
  await expect(review.auditRecord).toContainText(seeded.mentorName);
  const youth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const result = await request.get(`/api/me/attempts/${seeded.mentorQuizAttemptId}`, {
    headers: { Authorization: `Bearer ${youth.accessToken}` },
  });
  expect(result.ok(), await result.text()).toBeTruthy();
  expect((await result.json()).score).toBe(75);
});

test('approve all confirms and clears every pending quiz in scope', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/reviews');

  const queue = new MentorReviewsPage(page);
  await expect(queue.approveAllQuizzes).toContainText('1');
  page.once('dialog', (dialog) => dialog.accept());
  await queue.approveAllQuizzes.click();
  await expect(queue.status).toContainText('Approved 1 quiz assessment');
  await expect(
    queue.items.filter({ has: page.locator(`a[href$="${seeded.mentorQuizAttemptId}"]`) }),
  ).toHaveCount(0);
});
