import { expect, test } from '@playwright/test';
import { SignInPage, YouthBadgesPage, YouthJourneyPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-009
test('youth sees an authoritative roadmap, counted badge progress, and milestone timeline', async ({
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
  await expect(journey.heading).toBeVisible();
  await expect(journey.currentModule).toContainText(seeded.firstModuleName);
  await expect(journey.currentModule).toContainText('1 of 3 required items complete');
  await expect(page.getByText(`0 of 2 modules complete`)).toBeVisible();
  await expect(page.getByText(seeded.journeyBadgeName, { exact: true })).toBeVisible();
  await expect(page.getByText(seeded.codingBadgeName, { exact: true })).toBeVisible();

  await journey.badgeDetailsLink.click();
  const badges = new YouthBadgesPage(page);
  await expect(badges.heading).toBeVisible();
  const earned = page.getByRole('heading', { name: seeded.journeyBadgeName }).locator('..');
  await expect(earned).toContainText(/complete your first lesson/i);
  await expect(earned).toContainText(/earned/i);
  const coding = page.getByRole('heading', { name: seeded.codingBadgeName }).locator('..');
  await expect(coding).toContainText('0 of 2');
  await expect(page.getByText(/joined curriculum cohort/i)).toBeVisible();
  await expect(page.getByText(`${seeded.journeyBadgeName} badge`, { exact: true })).toBeVisible();
  await expect(page.getByText(/current goal/i)).toBeVisible();
});
