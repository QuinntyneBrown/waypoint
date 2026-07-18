import { expect, test } from '@playwright/test';
import { SignInPage, YouthBadgesPage, YouthJourneyPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-009, L2-NFR-009, L2-NFR-010
test('journey roadmap and badge collection reflow and remain keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto('/youth/journey');

  const journey = new YouthJourneyPage(page);
  await expect(journey.currentModule).toContainText(seeded.firstModuleName);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await journey.badgeDetailsLink.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await journey.badgeDetailsLink.focus();
  await page.keyboard.press('Enter');

  const badges = new YouthBadgesPage(page);
  await expect(badges.heading).toBeVisible();
  await expect(page.getByText(seeded.codingBadgeName, { exact: true })).toBeVisible();
  await expect(badges.progressLabels.filter({ hasText: '0 of 2' })).toHaveCount(1);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
