import { expect, test } from '@playwright/test';
import { SignInPage, YouthCodingPage, YouthSubmissionStatusPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '033-learning-experience-complete-coding-lessons';

test('capture: complete coding lessons', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/coding/${seeded.codingLessonId}`);

  const coding = new YouthCodingPage(page);
  await expect(coding.heading).toHaveText(seeded.codingLessonTitle);
  await expect(coding.editor).toHaveValue('<h1>Hello!</h1>');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'start');

  await coding.editor.fill('<h1>Welcome to Toronto</h1><p>I am learning to build for the web.</p>');
  await expect(page.getByRole('status')).toContainText(/workspace saved/i);
  await expect(
    page
      .frameLocator('[data-testid="coding-preview"]')
      .getByRole('heading', { name: 'Welcome to Toronto' }),
  ).toBeVisible();
  await page.getByTestId('coding-preview').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'preview');

  await page.getByRole('checkbox', { name: /welcome heading/i }).check();
  await coding.step(seeded.codingSecondStepTitle).click();
  await page.getByRole('checkbox', { name: /introduction paragraph/i }).check();
  await expect(coding.progress).toHaveText('100%');
  await expect(page.getByText(`Badge earned: ${seeded.codingBadgeName}`)).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await page.getByText('Self-check progress').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'done');

  await coding.mentorNote.fill('Please check whether my introduction is clear.');
  await coding.attestation.check();
  await shot(page, FOLDER, 'submit');
  page.once('dialog', (dialog) => dialog.accept());
  await coding.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  await expect(new YouthSubmissionStatusPage(page).status).toHaveText('Submitted');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'status');
});
