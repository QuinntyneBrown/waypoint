import { expect, test } from '@playwright/test';
import { SignInPage, YouthLessonPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-002, L2-NFR-009, L2-NFR-010
test('the lesson reader stays readable and keyboard operable on a mobile viewport', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await page.route('**/lesson-video.mp4', (route) => route.abort('failed'));
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/lessons/${seeded.secondLessonId}`);

  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  await expect(page.getByText(/full lesson text remains available/i)).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth - innerWidth)).toBeLessThanOrEqual(
    1,
  );

  for (const target of [lesson.largeTextButton, lesson.bookmarkButton, lesson.retryMediaButton]) {
    const box = await target.boundingBox();
    expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);
    expect(box?.width ?? 0).toBeGreaterThanOrEqual(44);
  }

  await lesson.largeTextButton.focus();
  await page.keyboard.press('Enter');
  await expect(lesson.largeTextButton).toHaveAttribute('aria-pressed', 'true');
  await expect(page.getByRole('status')).toHaveText('Reading preferences saved.');
});
