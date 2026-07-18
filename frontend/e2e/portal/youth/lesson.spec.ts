import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage, YouthLessonPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-002
test('youth reads a safe lesson, persists controls, retries media, and completes into reflection', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await page.route('**/lesson-video.mp4', (route) => route.abort('failed'));
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await curriculum.module(seeded.firstModuleName).getByText(seeded.secondLessonTitle).click();

  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  await expect(lesson.stepProgress).toContainText('Step 2 of 3');
  await expect(page.getByText('Luke 10:27', { exact: true })).toBeVisible();
  await expect(page.getByText(/put love into practice/i).first()).toBeVisible();
  await expect(page.getByText(seeded.leaderSecret)).toHaveCount(0);
  await expect(lesson.retryMediaButton).toBeVisible();

  await lesson.largeTextButton.click();
  await lesson.bookmarkButton.click();
  await expect(lesson.largeTextButton).toHaveAttribute('aria-pressed', 'true');
  await expect(lesson.bookmarkButton).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByRole('status')).toHaveText('Reading preferences saved.');

  await page.reload();
  await expect(lesson.largeTextButton).toHaveAttribute('aria-pressed', 'true');
  await expect(lesson.bookmarkButton).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByText(/put love into practice/i).first()).toBeVisible();

  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toMatch(/mark.*complete/i);
    await dialog.accept();
  });
  await lesson.completeButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}/reflection`));
});
