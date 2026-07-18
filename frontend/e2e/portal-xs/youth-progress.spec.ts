import { expect, test } from '@playwright/test';
import { SignInPage, YouthProgressPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-010, L2-NFR-009, L2-NFR-010
test('progress tabs and history tables stay contained and keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto('/youth/progress');

  const progress = new YouthProgressPage(page);
  await expect(progress.overviewPanel).toContainText(seeded.progressCompetencyName);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await progress.overviewTab.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await progress.overviewTab.focus();
  await page.keyboard.press('ArrowRight');
  await expect(progress.quizzesTab).toBeFocused();
  await expect(progress.quizzesTab).toHaveAttribute('aria-selected', 'true');
  await expect(progress.quizzesPanel).toContainText(seeded.progressQuizTitle);
  await page.keyboard.press('ArrowRight');
  await expect(progress.attendanceTab).toBeFocused();
  await expect(progress.attendancePanel).toContainText(seeded.progressPresentSessionTitle);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
