import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthAssignmentPage,
  YouthResubmitPage,
  YouthSubmissionStatusPage,
} from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-LEARN-006, L2-LEARN-007, L2-NFR-001
test('youth submits safe assignment evidence and follows revision through approval', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/assignments/${seeded.assignmentId}`);

  const assignment = new YouthAssignmentPage(page);
  await expect(assignment.heading).toHaveText(seeded.assignmentTitle);
  await expect(page.getByText(/practical act of neighbour-love/i)).toBeVisible();
  await expect(page.getByText(/due/i).first()).toBeVisible();
  await expect(page.getByText(/written response/i).first()).toBeVisible();
  await expect(page.getByText(/photo/i).first()).toBeVisible();
  await expect(page.getByText(seeded.assignmentCriterion, { exact: true })).toBeVisible();
  await expect(page.getByText('10 points', { exact: true })).toBeVisible();

  await assignment.submitButton.click();
  await expect(page.getByText(/confirm this is your own work/i)).toBeVisible();
  await assignment.photoInput.setInputFiles({
    name: 'unsafe.txt',
    mimeType: 'text/plain',
    buffer: Buffer.from('<script>alert(1)</script>'),
  });
  await expect(page.getByRole('alert')).toContainText(/jpg or png/i);

  await assignment.photoInput.setInputFiles({
    name: 'evidence.png',
    mimeType: 'image/png',
    buffer: Buffer.from(
      'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      'base64',
    ),
  });
  await expect(page.getByRole('status')).toContainText(/photo ready/i);
  await assignment.responseText.fill('I listened to a neighbour and helped carry their groceries.');
  await assignment.evidenceLink.fill('https://example.org/my-project');
  await assignment.attestation.check();
  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toMatch(/read-only/i);
    await dialog.accept();
  });
  await assignment.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  const submissionId = page.url().split('/').at(-1)!;

  const status = new YouthSubmissionStatusPage(page);
  await expect(status.heading).toHaveText(seeded.assignmentTitle);
  await expect(status.status).toHaveText('Submitted');
  await expect(page.getByText(/helped carry their groceries/i)).toBeVisible();
  await expect(page.getByText(/under review/i)).toBeVisible();

  const mentorAuth = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const revision = await request.post(`/api/mentor/submissions/${submissionId}/grade`, {
    data: {
      decision: 'request-revision',
      comment: 'Add the scripture that guided your action.',
      guardianNotified: false,
    },
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
  });
  expect(revision.ok(), await revision.text()).toBeTruthy();
  await page.reload();
  await expect(status.status).toHaveText('Needs revision');
  await expect(page.getByText('Add the scripture that guided your action.')).toBeVisible();
  await status.reviseLink.click();

  const resubmit = new YouthResubmitPage(page);
  await expect(resubmit.heading).toContainText(/revise/i);
  await expect(resubmit.responseText).toHaveValue(/helped carry their groceries/i);
  await resubmit.responseText.fill(
    'Luke 10:27 guided me to listen and help carry my neighbour’s groceries.',
  );
  await resubmit.submitButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/submissions/${submissionId}$`));
  await expect(status.status).toHaveText('Under review');
  await expect(page.getByText('Needs revision', { exact: true })).toBeVisible();
  await expect(page.getByText('Resubmitted', { exact: true })).toBeVisible();

  const approved = await request.post(`/api/mentor/submissions/${submissionId}/grade`, {
    data: {
      decision: 'approve',
      rubricScoresJson: JSON.stringify({ [seeded.assignmentCriterionId]: 9 }),
      totalAwarded: 9,
      comment: 'Clear connection between scripture and action.',
      guardianNotified: false,
    },
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
  });
  expect(approved.ok(), await approved.text()).toBeTruthy();
  await page.reload();
  await expect(status.status).toHaveText('Approved');
  await expect(page.getByText('9 of 10 points', { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.assignmentBadgeName, { exact: true })).toBeVisible();
  await expect(page.getByText('Clear connection between scripture and action.')).toBeVisible();
});
