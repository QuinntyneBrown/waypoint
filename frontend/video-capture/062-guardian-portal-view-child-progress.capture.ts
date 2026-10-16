import { Locator, expect, test } from '@playwright/test';
import { GuardianHomePage, GuardianProgressPage, SignInPage } from '../e2e/pages';
import { seedGuardianCast } from './guardian-cast';
import { shotFull } from './shotOf';
import { shot } from './shots';

const FOLDER = '062-guardian-portal-view-child-progress';

test('capture: view child progress', async ({ page, request }) => {
  test.setTimeout(120_000);
  const { guardian, family } = await seedGuardianCast(request, 'Imani Clarke');

  await new SignInPage(page).signIn(guardian);
  const home = new GuardianHomePage(page);
  await expect(home.heading).toBeVisible();
  await expect(home.childName).toContainText('Imani Clarke');
  await shot(page, FOLDER, 'home', { x: 244, y: 56, width: 1036, height: 560 });

  await home.progressLink.click();
  const progress = new GuardianProgressPage(page);
  await expect(progress.attendanceRows).toHaveCount(2);
  await expect(progress.sharedCheckIns).toContainText(family.sharedActionItems);
  await expect(page.getByText(family.privateSecret)).toHaveCount(0);
  await page.waitForLoadState('networkidle');

  const card = (title: string) => page.locator('section.w-card', { hasText: title });
  // Crop in document coordinates; the locked-badge list is long (it holds badges from other runs), so
  // each picture stops after the first lines of it.
  const docTop = async (target: Locator) =>
    (await target.boundingBox())!.y + (await page.evaluate(() => window.scrollY));
  const left = 244 + 20;
  const width = 1036 - 40;
  const modulesTop = await docTop(card('Modules'));
  const attendanceBox = (await card('Attendance history').boundingBox())!;
  await shotFull(page, FOLDER, 'modules-attendance', {
    x: left,
    y: modulesTop - 12,
    width,
    height: (await docTop(card('Attendance history'))) + attendanceBox.height - modulesTop + 24,
  });
  const quizTop = await docTop(page.getByTestId('guardian-quiz-outcomes'));
  await shotFull(page, FOLDER, 'quiz-badges', { x: left, y: quizTop - 12, width, height: 390 });
  const scheduleTop = await docTop(card('Upcoming schedule'));
  const updates = card('Shared mentor updates');
  const updatesBox = (await updates.boundingBox())!;
  await shotFull(page, FOLDER, 'schedule-updates', {
    x: left,
    y: scheduleTop - 12,
    width,
    height: (await docTop(updates)) + updatesBox.height - scheduleTop + 24,
  });
});
