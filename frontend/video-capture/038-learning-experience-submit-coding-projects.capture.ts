import { expect, test } from '@playwright/test';
import { SignInPage, YouthCodingPage, YouthSubmissionStatusPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '038-learning-experience-submit-coding-projects';

test('capture: submit coding project', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto(`/youth/coding/${seeded.codingLessonId}`);

  const coding = new YouthCodingPage(page);
  await expect(coding.heading).toHaveText(seeded.codingLessonTitle);
  await expect(coding.editor).toHaveValue('<h1>Hello!</h1>');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'lesson');

  await coding.editor.fill('<h1>Welcome to Toronto</h1><p>I am learning to build for the web.</p>');
  await expect(page.getByRole('status')).toContainText(/workspace saved/i);
  await expect(
    page
      .frameLocator('[data-testid="coding-preview"]')
      .getByRole('heading', { name: 'Welcome to Toronto' }),
  ).toBeVisible();
  await shot(page, FOLDER, 'preview');

  await page.getByRole('checkbox', { name: /welcome heading/i }).check();
  await coding.step(seeded.codingSecondStepTitle).click();
  await page.getByRole('checkbox', { name: /introduction paragraph/i }).check();
  await expect(coding.progress).toHaveText('100%');
  await expect(page.getByText(`Badge earned: ${seeded.codingBadgeName}`)).toBeVisible();
  await page.evaluate(
    () => document.querySelector('main')?.scrollTo(0, 0) ?? window.scrollTo(0, 0),
  );
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'complete');

  await coding.mentorNote.fill('Please check whether my introduction is clear.');
  await coding.attestation.check();
  await coding.submitButton.scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'submit');

  page.once('dialog', async (dialog) => dialog.accept());
  await coding.submitButton.click();
  await expect(page).toHaveURL(/\/youth\/submissions\/[^/]+$/);
  await expect(new YouthSubmissionStatusPage(page).status).toHaveText('Submitted');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'status');
});
