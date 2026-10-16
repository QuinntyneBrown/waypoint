import { expect, test } from '@playwright/test';
import { SignInPage, YouthProgressPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '035-learning-experience-review-personal-progress';

test('capture: review personal progress', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto('/youth/progress');

  const progress = new YouthProgressPage(page);
  await expect(progress.overviewPanel).toContainText(seeded.progressCompetencyName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'overview');

  await progress.quizzesTab.click();
  await expect(progress.quizzesPanel).toContainText(seeded.progressQuizTitle);
  await page.mouse.move(700, 650);
  await page.waitForTimeout(700);
  await shot(page, FOLDER, 'quizzes');

  await progress.attendanceTab.click();
  await expect(progress.attendancePanel).toContainText(seeded.progressAbsentSessionTitle);
  await page.mouse.move(700, 650);
  await page.waitForTimeout(700);
  await shot(page, FOLDER, 'attendance');
});
