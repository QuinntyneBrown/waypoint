import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-001
test('youth switches enrolled tracks and sees ordered server-gated curriculum items', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await expect(curriculum.heading).toBeVisible();
  await expect(curriculum.trackOptions).toHaveCount(2);
  await expect(curriculum.modules).toHaveCount(2);
  await expect(page.getByText('Draft module', { exact: true })).toHaveCount(0);

  const first = curriculum.module(seeded.firstModuleName);
  await expect(first).toContainText(/33%/);
  const itemTexts = await first.locator('[data-testid="curriculum-item"]').allTextContents();
  expect(itemTexts[0]).toContain(seeded.firstLessonTitle);
  expect(itemTexts[1]).toContain(seeded.secondLessonTitle);
  expect(itemTexts[2]).toContain('Module quiz');
  await expect(first.getByText(seeded.secondLessonTitle).locator('..')).not.toContainText(
    /locked/i,
  );
  await expect(first.getByText('Module quiz').locator('..')).toContainText(/complete all lessons/i);

  const locked = curriculum.module(seeded.lockedModuleName);
  await expect(locked).toContainText(new RegExp(`complete ${seeded.firstModuleName}`, 'i'));
  await expect(locked.getByRole('link')).toHaveCount(0);

  await curriculum.trackOptions.filter({ hasText: seeded.secondTrackName }).click();
  await expect(curriculum.emptyState).toBeVisible();
  await expect(curriculum.modules).toHaveCount(0);

  await curriculum.trackOptions.filter({ hasText: seeded.primaryTrackName }).click();
  await expect(curriculum.module(seeded.firstModuleName)).toBeVisible();
});
