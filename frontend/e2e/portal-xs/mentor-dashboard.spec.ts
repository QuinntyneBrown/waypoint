import { expect, test } from '@playwright/test';
import { MentorDashboardPage, MentorRosterPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-001, L2-NFR-009, L2-NFR-010
test('mentor dashboard and roster reflow with keyboard-sized controls on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });

  const dashboard = new MentorDashboardPage(page);
  const reviewItem = dashboard.reviewItem(seeded.mentorReviewTitle);
  await expect(reviewItem).toBeVisible();
  const reviewLink = reviewItem.getByRole('link', { name: /review/i });
  expect((await reviewLink.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  await dashboard.viewAllYouth.focus();
  await page.keyboard.press('Enter');
  const roster = new MentorRosterPage(page);
  await expect(roster.heading).toBeVisible();
  expect((await roster.filter('At risk').boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await roster.search.focus();
  await page.keyboard.type('No such youth');
  await expect(roster.noMatches).toBeVisible();
  await roster.clearFilters.focus();
  await page.keyboard.press('Enter');
  await expect(roster.rows).toContainText(seeded.youthName);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
