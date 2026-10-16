import { expect, test } from '@playwright/test';
import {
  MentorCompetencyPage,
  MentorQuizReviewPage,
  MentorReviewsPage,
  MentorSubmissionReviewPage,
  SignInPage,
} from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shotFull, shotOf } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/11-grade-and-assess';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };
const FULL = { x: 250, y: 70, width: 1030, height: 810 };

test('capture: grade and assess journey', async ({ page, request }) => {
  test.setTimeout(120000);
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

  await page.goto(`/mentor/reviews/submissions/${seeded.mentorReviewSubmissionId}`);
  const review = new MentorSubmissionReviewPage(page);
  await expect(review.heading).toHaveText(seeded.mentorReviewTitle);
  await expect(review.submittedCopy).toContainText('I helped welcome a new person');
  await page.waitForLoadState('networkidle');
  await shotFull(page, FOLDER, 'submission', FULL);

  await review.score(seeded.mentorReviewCriterion).fill('10');
  await expect(review.total).toHaveText('10 / 10');
  await review.comment.fill('Clear and specific. Well done.');
  await shotFull(page, FOLDER, 'ready', FULL);
  page.once('dialog', (dialog) => dialog.accept());
  await review.approve.click();
  await expect(page).toHaveURL(/\/mentor\/reviews$/);
  await page.waitForLoadState('networkidle');

  await quizItem.getByRole('link', { name: /review/i }).click();
  const quiz = new MentorQuizReviewPage(page);
  await expect(quiz.autoScore).toHaveText(`${seeded.mentorQuizAutoScore}%`);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'quiz', MAIN);

  await quiz.overrideScore.fill('75');
  page.once('dialog', (dialog) => dialog.accept());
  await quiz.approve.click();
  await expect(quiz.auditRecord).toContainText('Approved 75%');
  await quiz.auditRecord.scrollIntoViewIfNeeded();
  await shotOf(page, FOLDER, 'quiz-record', quiz.auditRecord, 16, 700, 300);

  await page.goto(`/mentor/youth/${seeded.youthId}/competencies`);
  const tracker = new MentorCompetencyPage(page);
  const id = seeded.mentorCompetencyId;
  await expect(tracker.heading).toContainText('Amara Bell');
  await tracker.levelById(id).scrollIntoViewIfNeeded();
  await tracker.levelById(id).selectOption('Proficient');
  await tracker.notesById(id).fill('Took initiative welcoming a new participant.');
  await tracker.saveById(id).click();
  await expect(tracker.status).toContainText(`Saved ${seeded.mentorCompetencyName}`);
  await expect(tracker.cardById(id)).toContainText('Grace Adeyemi');
  await shotOf(page, FOLDER, 'competency', tracker.cardById(id));
});
