import { expect, test } from '@playwright/test';
import { MentorSubmissionReviewPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';
import { shotFull } from './shotOf';

const FOLDER = '054-mentoring-grade-submissions';
const FULL = { x: 250, y: 70, width: 1030, height: 880 };

test('capture: grade submissions', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto(`/mentor/reviews/submissions/${seeded.mentorReviewSubmissionId}`);

  const review = new MentorSubmissionReviewPage(page);
  await expect(review.heading).toHaveText(seeded.mentorReviewTitle);
  await expect(review.submittedCopy).toContainText('I helped welcome a new person');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'submission', FULL);

  await review.score(seeded.mentorReviewCriterion).fill('7');
  await expect(review.total).toHaveText('7 / 10');
  await expect(review.guardianNotify).toBeChecked();
  await review.requestRevision.click();
  await expect(page.getByRole('alert')).toContainText('Feedback is required');
  await shotFull(page, FOLDER, 'revision-note', FULL);

  await review.comment.fill('Strong start; make the leadership link clearer.');
  await review.saveDraft.click();
  await expect(review.status).toContainText('Draft saved');
  await shotFull(page, FOLDER, 'draft', FULL);

  await review.comment.fill('Strong start; make the leadership link clearer.');
  await review.score(seeded.mentorReviewCriterion).fill('10');
  await expect(review.total).toHaveText('10 / 10');
  await shotFull(page, FOLDER, 'ready', FULL);

  page.once('dialog', (dialog) => dialog.accept());
  await review.approve.click();
  await expect(page).toHaveURL(/\/mentor\/reviews$/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'queue', { x: 380, y: 80, width: 760, height: 460 });
});
