import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthAssignmentPage,
  YouthResubmitPage,
  YouthSubmissionStatusPage,
} from '../../e2e/pages';
import { signInApi } from '../../e2e/support/api';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/10-submit-assignments';

test('capture: journey 10 submit assignments', async ({ page, request }) => {
  const seeded = await seedCast(request, 'Amara Bell');
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.goto(`/youth/assignments/${seeded.assignmentId}`);

  const assignment = new YouthAssignmentPage(page);
  await expect(assignment.heading).toHaveText(seeded.assignmentTitle);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'brief');

  await assignment.submitButton.click();
  await expect(page.getByText(/confirm this is your own work/i)).toBeVisible();
  await assignment.photoInput.setInputFiles({
    name: 'unsafe.txt',
    mimeType: 'text/plain',
    buffer: Buffer.from('not a photo'),
  });
  await expect(page.getByRole('alert').first()).toContainText(/jpg or png/i);
  await assignment.photoInput.scrollIntoViewIfNeeded();
  await page.getByRole('alert').first().scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'validation');

  await page.reload();
  await expect(assignment.heading).toHaveText(seeded.assignmentTitle);
  await assignment.responseText.fill('I listened to a neighbour and helped carry their groceries.');
  await assignment.attestation.check();
  await assignment.submitButton.scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'ready');

  page.once('dialog', (dialog) => dialog.accept());
  await assignment.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  const submissionId = page.url().split('/').at(-1)!;
  const status = new YouthSubmissionStatusPage(page);
  await expect(status.status).toHaveText('Submitted');
  await expect(page.getByText(/under review/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await shot(page, FOLDER, 'submitted');

  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const headers = { Authorization: `Bearer ${mentorAuth.accessToken}` };
  const versionOf = async () => {
    const res = await request.get(`/api/mentor/submissions/${submissionId}`, { headers });
    return ((await res.json()) as { version: string }).version;
  };
  const revision = await request.post(`/api/mentor/submissions/${submissionId}/grade`, {
    headers,
    data: {
      decision: 'request-revision',
      comment: 'Add the scripture that guided your action.',
      guardianNotified: false,
      expectedVersion: await versionOf(),
    },
  });
  expect(revision.ok(), await revision.text()).toBeTruthy();
  await page.reload();
  await expect(status.status).toHaveText('Needs revision');
  await expect(page.getByText('Add the scripture that guided your action.')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await shot(page, FOLDER, 'revision');

  await status.reviseLink.click();
  const resubmit = new YouthResubmitPage(page);
  await expect(resubmit.heading).toContainText(/revise/i);
  await resubmit.responseText.fill(
    'Luke 10:27 guided me to listen and help carry my neighbour’s groceries.',
  );
  await resubmit.submitButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/submissions/${submissionId}$`));
  await expect(status.status).toHaveText('Under review');

  const approved = await request.post(`/api/mentor/submissions/${submissionId}/grade`, {
    headers,
    data: {
      decision: 'approve',
      rubricScoresJson: JSON.stringify({ [seeded.assignmentCriterionId]: 9 }),
      totalAwarded: 9,
      comment: 'Clear connection between scripture and action.',
      guardianNotified: false,
      expectedVersion: await versionOf(),
    },
  });
  expect(approved.ok(), await approved.text()).toBeTruthy();
  await page.reload();
  await expect(status.status).toHaveText('Approved');
  await expect(page.getByText('9 of 10 points', { exact: true })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await shot(page, FOLDER, 'approved');
});
