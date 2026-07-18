import { expect, test } from '@playwright/test';
import { SignInPage, YouthProgressPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-010
test('youth reviews overview, approved quiz history, attendance, and mentor competency evidence', async ({
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
  await expect(progress.heading).toBeVisible();
  await expect(progress.overviewPanel).toContainText('50%');
  await expect(progress.overviewPanel).toContainText('82%');
  await expect(progress.overviewPanel).toContainText('1/2');
  await expect(progress.overviewPanel).toContainText(seeded.progressCompetencyName);
  await expect(progress.overviewPanel).toContainText('Proficient');
  await expect(progress.overviewPanel).toContainText('75%');

  await progress.quizzesTab.click();
  await expect(progress.quizzesPanel).toContainText(seeded.progressQuizTitle);
  await expect(progress.quizzesPanel).toContainText('82%');
  await expect(progress.quizzesPanel).toContainText('Passed');

  await progress.attendanceTab.click();
  await expect(progress.attendancePanel).toContainText(seeded.progressPresentSessionTitle);
  await expect(progress.attendancePanel).toContainText('Present');
  await expect(progress.attendancePanel).toContainText(seeded.progressAbsentSessionTitle);
  await expect(progress.attendancePanel).toContainText('Absent');
  await expect(progress.attendancePanel).toContainText('50%');
});
