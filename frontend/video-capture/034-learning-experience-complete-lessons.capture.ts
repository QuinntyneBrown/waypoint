import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage, YouthLessonPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '034-learning-experience-complete-lessons';

test('capture: complete lessons', async ({ page, request }) => {
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
  await expect(lesson.retryMediaButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'lesson');

  await lesson.largeTextButton.click();
  await lesson.bookmarkButton.click();
  await expect(page.getByRole('status')).toHaveText('Reading preferences saved.');
  await shot(page, FOLDER, 'preferences');

  await lesson.retryMediaButton.scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'media');

  page.once('dialog', (dialog) => dialog.accept());
  await lesson.completeButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}/reflection`));
  await expect(page.getByLabel(/your reflection/i)).toBeVisible();
  await shot(page, FOLDER, 'reflection');
});
