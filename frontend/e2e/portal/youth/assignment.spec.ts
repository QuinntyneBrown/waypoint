import { expect, test } from '@playwright/test';
import {
  SignInPage,
  YouthAssignmentPage,
  YouthResubmitPage,
  YouthSubmissionStatusPage,
} from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

const evidencePng = {
  name: 'evidence.png',
  mimeType: 'image/png',
  buffer: Buffer.from(
    'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
    'base64',
  ),
};

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

  await assignment.photoInput.setInputFiles(evidencePng);
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

// Traces to: L2-CONSENT-001 AC2/AC4, L2-CONSENT-004 AC3
test('individual evidence media follows granular guardian consent and later withdrawal', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const guardian = await signInApi(request, {
    email: seeded.guardianEmail,
    password: seeded.youthPassword,
  });
  const updateMediaConsent = async (allPhotosVideo: boolean, groupOnly: boolean) => {
    const response = await request.put(`/api/guardian/children/${seeded.youthId}/media-consent`, {
      headers: { Authorization: `Bearer ${guardian.accessToken}` },
      data: {
        mediaConsent: allPhotosVideo || groupOnly,
        mediaAllPhotosVideo: allPhotosVideo,
        mediaGroupOnly: groupOnly,
        mediaFirstNameCaptions: false,
      },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
  };

  await updateMediaConsent(false, true);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/assignments/${seeded.assignmentId}`);
  const assignment = new YouthAssignmentPage(page);
  await expect(assignment.heading).toHaveText(seeded.assignmentTitle);

  await assignment.photoInput.setInputFiles(evidencePng);
  await expect(page.getByRole('alert')).toContainText(
    /guardian consent for individual photos or video is required/i,
  );
  await expect(page).toHaveURL(new RegExp(`/youth/assignments/${seeded.assignmentId}$`));

  await updateMediaConsent(true, false);
  await assignment.photoInput.setInputFiles([]);
  await assignment.photoInput.setInputFiles(evidencePng);
  await expect(page.getByRole('status')).toContainText(/photo ready/i);
  await assignment.responseText.fill('Private evidence with current guardian media consent.');
  await assignment.attestation.check();
  page.once('dialog', async (dialog) => dialog.accept());
  await assignment.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  await expect(page.getByRole('img', { name: /submitted evidence/i })).toBeVisible();

  await updateMediaConsent(false, false);
  await page.reload();
  await expect(page.getByRole('img', { name: /submitted evidence/i })).toHaveCount(0);
  await expect(
    page.getByText(/private evidence with current guardian media consent/i),
  ).toBeVisible();
});

// Traces to: L2-LEARN-006 AC2/AC4, L2-NFR-001 AC4
test('youth submits a sniffed private PDF and downloads it from the submitted copy', async ({
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
  await expect(page.getByText('File (PDF)', { exact: true })).toBeVisible();

  await assignment.fileInput.setInputFiles({
    name: 'evidence.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('<script>alert(1)</script>'),
  });
  await expect(page.getByRole('alert')).toContainText(/valid pdf/i);

  await assignment.fileInput.setInputFiles({
    name: 'evidence.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF'),
  });
  await expect(page.getByRole('status')).toContainText(/file ready: evidence\.pdf/i);
  await assignment.responseText.fill('The supporting document is attached.');
  await assignment.attestation.check();
  page.once('dialog', async (dialog) => dialog.accept());
  await assignment.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);

  const status = new YouthSubmissionStatusPage(page);
  await expect(status.evidenceFileLink).toBeVisible();
  const download = page.waitForEvent('download');
  await status.evidenceFileLink.click();
  expect((await download).suggestedFilename()).toBe('evidence.pdf');
});
