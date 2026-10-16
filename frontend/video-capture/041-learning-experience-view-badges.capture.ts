import { expect, test } from '@playwright/test';
import { SignInPage, YouthBadgesPage, YouthJourneyPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '041-learning-experience-view-badges';

test('capture: view badges', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto('/youth/journey');
  await expect(new YouthJourneyPage(page).badgeDetailsLink).toBeVisible();
  await new YouthJourneyPage(page).badgeDetailsLink.click();
  const badges = new YouthBadgesPage(page);
  await expect(badges.heading).toBeVisible();
  await expect(page.getByText(/joined curriculum cohort/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'badges', { x: 244, y: 70, width: 1036, height: 620 });

  await page.getByRole('heading', { name: seeded.codingBadgeName }).scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'locked', { x: 244, y: 290, width: 1036, height: 340 });

  await page.getByText(/joined curriculum cohort/i).scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'timeline', { x: 244, y: 370, width: 1036, height: 410 });
});
