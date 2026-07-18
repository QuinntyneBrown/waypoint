import { expect, test } from '@playwright/test';
import { GuardianHomePage, GuardianProgressPage, SignInPage } from '../../pages';
import { registerUser, seedGuardianFamily } from '../../support/api';

// Traces to: L2-GUARDIAN-001
test('a guardian sees only their linked child aggregate and explicitly shared check-ins', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-progress');
  const family = await seedGuardianFamily(request, guardian.userId);

  await new SignInPage(page).signIn(guardian);
  const home = new GuardianHomePage(page);
  await expect(page).toHaveURL(GuardianHomePage.urlPattern);
  await expect(home.heading).toBeVisible();
  await expect(home.childName).toContainText(family.childName);
  await expect(home.overallProgress).toContainText('50%');

  await home.progressLink.click();
  const progress = new GuardianProgressPage(page);
  await expect(page).toHaveURL(GuardianProgressPage.urlPattern);
  await expect(progress.heading).toBeVisible();
  await expect(progress.moduleRows.filter({ hasText: family.moduleName })).toContainText('50%');
  await expect(progress.attendanceRows).toHaveCount(2);
  await expect(progress.attendanceRows.filter({ hasText: 'Present' })).toHaveCount(1);
  await expect(progress.attendanceRows.filter({ hasText: 'Absent' })).toHaveCount(1);
  await expect(progress.earnedBadges).toContainText(family.earnedBadgeName);
  await expect(progress.lockedBadges).toContainText(family.lockedBadgeName);
  await expect(progress.upcomingSessions).toContainText(family.upcomingTitle);
  await expect(progress.sharedCheckIns).toContainText(family.sharedActionItems);

  await expect(page.getByText(family.privateSecret)).toHaveCount(0);
  await expect(page.getByText(/lesson content|quiz answer|private reflection/i)).toHaveCount(0);
  await expect(
    page.getByRole('button', { name: /edit child|edit progress|change grade/i }),
  ).toHaveCount(0);
});

test('a guardian with no linked child sees a safe empty state', async ({ page, request }) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-empty');
  await new SignInPage(page).signIn(guardian);

  await expect(page).toHaveURL(GuardianHomePage.urlPattern);
  await expect(page.getByRole('heading', { name: /no linked youth yet/i })).toBeVisible();
});
