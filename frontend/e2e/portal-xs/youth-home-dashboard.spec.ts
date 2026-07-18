import { expect, test } from '@playwright/test';
import { SignInPage, YouthHomePage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-001..010, L2-NFR-009, L2-NFR-010
test('placed youth dashboard reflows without overflow and keeps primary actions touch-sized', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const home = new YouthHomePage(page);
  await expect(home.placedDashboard).toContainText(seeded.scheduleSessionTitle);
  await expect(home.placedDashboard).toContainText(seeded.assignmentTitle);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await home.continueLearningLink.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await home.continueLearningLink.focus();
  await expect(home.continueLearningLink).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/youth\/(lessons|coding|quizzes)\//);
});
