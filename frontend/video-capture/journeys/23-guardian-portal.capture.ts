import { Locator, expect, test } from '@playwright/test';
import {
  GuardianConsentPage,
  GuardianHomePage,
  GuardianProgressPage,
  SignInPage,
} from '../../e2e/pages';
import { seedGuardianCast, shotElement } from '../guardian-cast';
import { shotFull } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/23-guardian-portal';

test('capture: guardian portal', async ({ page, request }) => {
  test.setTimeout(120_000);
  const { guardian, family } = await seedGuardianCast(request, 'Imani Clarke');

  await new SignInPage(page).signIn(guardian);
  const home = new GuardianHomePage(page);
  await expect(home.heading).toBeVisible();
  await expect(home.childName).toContainText('Imani Clarke');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'home', { x: 244, y: 56, width: 1036, height: 560 });

  await home.progressLink.click();
  const progress = new GuardianProgressPage(page);
  await expect(progress.attendanceRows).toHaveCount(2);
  await expect(progress.sharedCheckIns).toContainText(family.sharedActionItems);
  await page.waitForLoadState('networkidle');

  const card = (title: string) => page.locator('section.ph-card', { hasText: title });
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
  const scheduleTop = await docTop(card('Upcoming schedule'));
  const updates = card('Shared mentor updates');
  const updatesBox = (await updates.boundingBox())!;
  await shotFull(page, FOLDER, 'schedule-updates', {
    x: left,
    y: scheduleTop - 12,
    width,
    height: (await docTop(updates)) + updatesBox.height - scheduleTop + 24,
  });

  const consent = new GuardianConsentPage(page);
  await consent.goto();
  await expect(consent.referenceId).toContainText(/CONSENT-/);
  await page.waitForLoadState('networkidle');
  await shotElement(page, FOLDER, 'consent', card('Participation consent'));
});
