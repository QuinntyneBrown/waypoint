import { expect, test } from '@playwright/test';
import { SignInPage, YouthCodingPage, YouthCurriculumPage, YouthLessonPage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/01-complete-lessons';

test('capture: journey 01 complete lessons', async ({ page, request }) => {
  const seeded = await seedCast(request, 'Amara Bell');
  await page.route('**/lesson-video.mp4', (route) => route.abort('failed'));
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });

  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await expect(curriculum.modules).toHaveCount(2);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'map');

  await curriculum.module(seeded.firstModuleName).getByText('Module quiz').scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollBy(0, 330));
  await shot(page, FOLDER, 'gated');

  await curriculum.module(seeded.firstModuleName).getByText(seeded.secondLessonTitle).click();
  const lesson = new YouthLessonPage(page);
  await expect(lesson.heading).toHaveText(seeded.secondLessonTitle);
  await expect(lesson.retryMediaButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'lesson');

  page.once('dialog', (dialog) => dialog.accept());
  await lesson.completeButton.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}/reflection`));
  await expect(page.getByLabel(/your reflection/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'reflection');

  await curriculum.goto();
  await expect(curriculum.modules).toHaveCount(2);
  await page.waitForLoadState('networkidle');
  await curriculum.module(seeded.firstModuleName).getByText('Module quiz').scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollBy(0, 330));
  await shot(page, FOLDER, 'unlocked');

  await page.goto(`/youth/coding/${seeded.codingLessonId}`);
  const coding = new YouthCodingPage(page);
  await expect(coding.heading).toHaveText(seeded.codingLessonTitle);
  await page.waitForLoadState('networkidle');
  await coding.editor.fill('<h1>Welcome to Toronto</h1><p>I am learning to build for the web.</p>');
  await expect(page.getByRole('status')).toContainText(/workspace saved/i);
  await expect(
    page
      .frameLocator('[data-testid="coding-preview"]')
      .getByRole('heading', { name: 'Welcome to Toronto' }),
  ).toBeVisible();
  await page.getByTestId('coding-preview').scrollIntoViewIfNeeded();
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'coding');
});
