import { expect, test } from '@playwright/test';
import { SignInPage, YouthAssignmentPage, YouthSubmissionStatusPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '037-learning-experience-submit-assignments';

test('capture: submit assignments', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
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
  await assignment.attestation.check();
  await page.getByRole('status').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'ready');

  page.once('dialog', (dialog) => dialog.accept());
  await assignment.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  await expect(new YouthSubmissionStatusPage(page).status).toHaveText('Submitted');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'status');
});
