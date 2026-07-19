import { expect, test } from '@playwright/test';
import { MentorReviewsPage, MentorSubmissionReviewPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-003, L2-LEARN-007, L2-ENGAGE-008
test('mentor saves a rubric draft then approves with youth outcomes', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/reviews');

  const queue = new MentorReviewsPage(page);
  const item = queue.items.filter({ hasText: seeded.mentorReviewTitle });
  await expect(item).toContainText(seeded.youthName);
  await item.getByRole('link', { name: /review/i }).click();

  const review = new MentorSubmissionReviewPage(page);
  await expect(review.heading).toHaveText(seeded.mentorReviewTitle);
  await expect(review.submittedCopy).toContainText('I helped welcome a new person');
  await review.score(seeded.mentorReviewCriterion).fill('7');
  await expect(review.total).toHaveText('7 / 10');
  await review.comment.fill('Strong start; make the leadership link clearer.');
  await review.saveDraft.click();
  await expect(review.status).toContainText('Draft saved');

  await page.reload();
  await expect(review.score(seeded.mentorReviewCriterion)).toHaveValue('7');
  await expect(review.comment).toHaveValue('Strong start; make the leadership link clearer.');
  await review.score(seeded.mentorReviewCriterion).fill('10');
  page.once('dialog', (dialog) => dialog.accept());
  await review.approve.click();
  await expect(page).toHaveURL(/\/mentor\/reviews$/);
  await expect(queue.items.filter({ hasText: seeded.mentorReviewTitle })).toHaveCount(0);

  const youth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const statusResponse = await request.get(
    `/api/me/submissions/${seeded.mentorReviewSubmissionId}`,
    { headers: { Authorization: `Bearer ${youth.accessToken}` } },
  );
  expect(statusResponse.ok()).toBeTruthy();
  const status = (await statusResponse.json()) as { status: string; totalAwarded: number };
  expect(status.status).toBe('Approved');
  expect(status.totalAwarded).toBe(10);
  const notifications = await request.get('/api/notifications', {
    headers: { Authorization: `Bearer ${youth.accessToken}` },
  });
  expect(await notifications.text()).toContain('Assignment approved');
});

test('revision requires feedback and defaults guardian notification on', async ({
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
  await expect(review.guardianNotify).toBeChecked();
  await review.score(seeded.mentorReviewCriterion).fill('5');
  await review.requestRevision.click();
  await expect(page.getByRole('alert')).toContainText('Feedback is required');
  await review.comment.fill('Please explain how this changed your leadership practice.');
  page.once('dialog', (dialog) => dialog.accept());
  await review.requestRevision.click();
  await expect(page).toHaveURL(/\/mentor\/reviews$/);
});

// Traces to: L2-NFR-006 (stale grading reconciliation preserves the local draft)
test('mentor reconciles a stale grading draft without a silent overwrite', async ({
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
  await expect(review.heading).toHaveText(seeded.mentorReviewTitle);

  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const currentResponse = await request.get(
    `/api/mentor/submissions/${seeded.mentorReviewSubmissionId}`,
    { headers: { Authorization: `Bearer ${mentorAuth.accessToken}` } },
  );
  expect(currentResponse.ok(), await currentResponse.text()).toBeTruthy();
  const current = (await currentResponse.json()) as { version: string };
  expect(current.version).toBeTruthy();
  const winningSave = await request.post(
    `/api/mentor/submissions/${seeded.mentorReviewSubmissionId}/grade`,
    {
      headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
      data: {
        decision: 'save-draft',
        rubricScoresJson: JSON.stringify({ [seeded.mentorReviewCriterionId]: 8 }),
        comment: 'Feedback saved by another editor.',
        guardianNotified: false,
        expectedVersion: current.version,
      },
    },
  );
  expect(winningSave.ok(), await winningSave.text()).toBeTruthy();

  await review.score(seeded.mentorReviewCriterion).fill('4');
  await review.comment.fill('Keep this local draft while I reconcile.');
  await review.saveDraft.click();

  await expect(review.conflict).toContainText(/changed on the server/i);
  await expect(review.score(seeded.mentorReviewCriterion)).toHaveValue('4');
  await expect(review.comment).toHaveValue('Keep this local draft while I reconcile.');
  await review.loadLatest.click();
  await expect(review.score(seeded.mentorReviewCriterion)).toHaveValue('8');
  await expect(review.comment).toHaveValue('Feedback saved by another editor.');
});
