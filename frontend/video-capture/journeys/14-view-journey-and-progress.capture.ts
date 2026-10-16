import { expect, test } from '@playwright/test';
import { SignInPage, YouthBadgesPage, YouthJourneyPage, YouthProgressPage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/14-view-journey-and-progress';

test('capture: journey 14 view journey and progress', async ({ page, request }) => {
  const seeded = await seedCast(request, 'Amara Bell');
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.goto('/youth/journey');

  const journey = new YouthJourneyPage(page);
  await expect(journey.heading).toBeVisible();
  await expect(journey.currentModule).toContainText('1 of 3 required items complete');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'journey');

  await page.getByRole('heading', { name: 'Your roadmap' }).evaluate((el) => el.scrollIntoView());
  await page.evaluate(() => window.scrollBy(0, -80));
  await shot(page, FOLDER, 'roadmap');

  await page
    .getByRole('heading', { name: 'Badge collection' })
    .evaluate((el) => el.scrollIntoView());
  await page.evaluate(() => window.scrollBy(0, -80));
  await shot(page, FOLDER, 'collection', { x: 390, y: 50, width: 740, height: 250 });

  await journey.badgeDetailsLink.click();
  const badges = new YouthBadgesPage(page);
  await expect(badges.heading).toBeVisible();
  await expect(page.getByText(/joined curriculum cohort/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'badges', { x: 244, y: 70, width: 1036, height: 620 });

  await page.goto('/youth/progress');
  const progress = new YouthProgressPage(page);
  await expect(progress.overviewPanel).toContainText(seeded.progressCompetencyName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'progress');
});
