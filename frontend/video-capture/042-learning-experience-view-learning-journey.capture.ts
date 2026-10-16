import { expect, test } from '@playwright/test';
import { SignInPage, YouthJourneyPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '042-learning-experience-view-learning-journey';

test('capture: view learning journey', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto('/youth/journey');

  const journey = new YouthJourneyPage(page);
  await expect(journey.heading).toBeVisible();
  await expect(journey.currentModule).toContainText('1 of 3 required items complete');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'journey');

  await page.getByRole('heading', { name: 'Badge collection' }).scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollBy(0, -80));
  await shot(page, FOLDER, 'collection');

  await page.getByRole('heading', { name: 'Your roadmap' }).scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollBy(0, -80));
  await shot(page, FOLDER, 'roadmap');
});
