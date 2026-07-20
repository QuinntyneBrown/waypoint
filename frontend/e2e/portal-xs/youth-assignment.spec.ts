import { expect, test } from '@playwright/test';
import { SignInPage, YouthAssignmentPage, YouthSubmissionStatusPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-006, L2-LEARN-007, L2-NFR-009, L2-NFR-010
test('assignment brief, submission, and status stay contained and keyboard operable on mobile', async ({
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
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await assignment.submitButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await assignment.responseText.focus();
  await page.keyboard.type('I helped a neighbour and reflected on Luke 10:27.');
  await assignment.attestation.focus();
  await page.keyboard.press('Space');
  page.once('dialog', async (dialog) => dialog.accept());
  await assignment.submitButton.focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);

  const status = new YouthSubmissionStatusPage(page);
  await expect(status.status).toHaveText('Submitted');
  await expect(page.getByText(/helped a neighbour/i)).toBeVisible();
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
