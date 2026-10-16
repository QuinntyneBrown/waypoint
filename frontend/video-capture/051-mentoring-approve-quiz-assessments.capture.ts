import { expect, test } from '@playwright/test';
import { MentorQuizReviewPage, MentorReviewsPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';

const FOLDER = '051-mentoring-approve-quiz-assessments';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: approve quiz assessments', async ({ page: first, request, browser }) => {
  test.setTimeout(120000);
  let page = first;
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto('/mentor/reviews');

  const queue = new MentorReviewsPage(page);
  const quizItem = queue.items.filter({
    has: page.locator(`a[href$="${seeded.mentorQuizAttemptId}"]`),
  });
  await expect(quizItem).toContainText('Amara Bell');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'queue', { x: 380, y: 80, width: 760, height: 460 });

  await quizItem.getByRole('link', { name: /review/i }).click();
  const review = new MentorQuizReviewPage(page);
  await expect(review.autoScore).toHaveText(`${seeded.mentorQuizAutoScore}%`);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'review', MAIN);

  await review.overrideScore.fill('101');
  await review.approve.click();
  await expect(page.getByRole('alert')).toContainText('between 0 and 100');
  await shot(page, FOLDER, 'range', MAIN);

  await review.overrideScore.fill('75');
  page.once('dialog', (dialog) => dialog.accept());
  await review.approve.click();
  await expect(review.auditRecord).toContainText('Approved 75%');
  await review.auditRecord.scrollIntoViewIfNeeded();
  const box = (await review.auditRecord.boundingBox())!;
  await shot(page, FOLDER, 'record', {
    x: Math.max(0, box.x - 16),
    y: Math.max(0, box.y - 16),
    width: Math.min(700, box.width + 200),
    height: Math.min(300, box.height + 60),
  });

  // Approve all, from a fresh fixture so one pending quiz is in scope.
  const second = await seedCast(request, 'Amara Bell');
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(fresh).signIn({
    email: second.mentorEmail,
    password: second.mentorPassword,
  });
  page = fresh;
  const queue2 = new MentorReviewsPage(page);
  await page.goto('/mentor/reviews');
  await expect(queue2.approveAllQuizzes).toContainText('1');
  page.once('dialog', (dialog) => dialog.accept());
  await queue2.approveAllQuizzes.click();
  await expect(queue2.status).toContainText('Approved 1 quiz assessment');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'approved-all', { x: 380, y: 80, width: 760, height: 460 });
});
