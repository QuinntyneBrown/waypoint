import { expect, test } from '@playwright/test';
import { SignInPage, YouthLessonPage, YouthReflectionPage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/26-write-reflections';

test('capture: journey 26 write reflections', async ({ page, request }) => {
  const seeded = await seedCast(request, 'Amara Bell');
  await page.route('**/lesson-video.mp4', (route) => route.abort('failed'));
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });

  await page.goto(`/youth/lessons/${seeded.secondLessonId}`);
  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  page.once('dialog', (dialog) => dialog.accept());
  await lesson.completeButton.click();
  const reflectionUrl = `/youth/lessons/${seeded.secondLessonId}/reflection`;
  await expect(page).toHaveURL(new RegExp(reflectionUrl));

  const reflection = new YouthReflectionPage(page);
  await expect(reflection.heading).toContainText(seeded.secondLessonTitle);
  await expect(reflection.shareWithMentor).not.toBeChecked();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'empty');

  await reflection.submitButton.click();
  await expect(page.getByText(/reflection text is required/i)).toBeVisible();
  await expect(page.getByText(/choose how you are feeling/i)).toBeVisible();
  await shot(page, FOLDER, 'required');

  await reflection.reflectionText.fill('I can listen before I try to solve the problem.');
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await reflection.optionalResponse.fill('Ask my friend what support would help.');
  await page.evaluate(() => window.scrollTo(0, 330));
  await shot(page, FOLDER, 'filled');
  await reflection.shareWithMentor.scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'share');

  page.once('dialog', async (dialog) => dialog.accept());
  await reflection.backToLessonLink.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}$`));

  await page.goto(reflectionUrl);
  await expect(reflection.reflectionText).toHaveValue(
    'I can listen before I try to solve the problem.',
  );
  await expect(page.getByRole('status')).toContainText(/draft restored/i);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'restored');

  await reflection.shareWithMentor.check();
  await reflection.submitButton.click();
  await expect(page.getByRole('status')).toContainText(/reflection submitted/i);
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'submitted');
});
