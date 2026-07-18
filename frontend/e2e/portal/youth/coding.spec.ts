import { expect, test } from '@playwright/test';
import { SignInPage, YouthCodingPage, YouthSubmissionStatusPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-008
test('youth completes a resumable coding lesson and submits the project for mentor review', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/coding/${seeded.codingLessonId}`);

  const coding = new YouthCodingPage(page);
  await expect(coding.heading).toHaveText(seeded.codingLessonTitle);
  await expect(coding.editor).toHaveValue('<h1>Hello!</h1>');
  await expect(page.getByText('<h1>Welcome to my page</h1>', { exact: true })).toBeVisible();
  await coding.editor.fill('<h1>Welcome to Toronto</h1><p>I am learning to build for the web.</p>');
  await expect(page.getByRole('status')).toContainText(/workspace saved/i);
  await expect(
    page
      .frameLocator('[data-testid="coding-preview"]')
      .getByRole('heading', { name: 'Welcome to Toronto' }),
  ).toBeVisible();

  await page.getByRole('checkbox', { name: /welcome heading/i }).check();
  await coding.step(seeded.codingSecondStepTitle).click();
  await expect(
    page.getByText('<p>I am learning to build for the web.</p>', { exact: true }),
  ).toBeVisible();
  await page.getByRole('checkbox', { name: /introduction paragraph/i }).check();
  await expect(coding.progress).toHaveText('100%');
  await expect(page.getByText(`Badge earned: ${seeded.codingBadgeName}`)).toBeVisible();

  await page.reload();
  await expect(coding.editor).toHaveValue(/Welcome to Toronto/);
  await expect(coding.progress).toHaveText('100%');

  await coding.mentorNote.fill('Please check whether my introduction is clear.');
  await coding.attestation.check();
  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toMatch(/mentor.*review/i);
    await dialog.accept();
  });
  await coding.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);

  const status = new YouthSubmissionStatusPage(page);
  await expect(status.status).toHaveText('Submitted');
  await expect(page.getByText(/Welcome to Toronto/)).toBeVisible();
  await expect(page.getByText('Please check whether my introduction is clear.')).toBeVisible();
  await expect(page.getByText(/under review/i)).toBeVisible();
});
