import { expect, test } from '@playwright/test';
import { MentorDashboardPage, MentorRosterPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-MENTOR-001, L2-AUTH-010
test('mentor sees only their dashboard, review preview, and filterable roster', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });

  const dashboard = new MentorDashboardPage(page);
  await expect(page).toHaveURL(/\/mentor\/dashboard/);
  await expect(dashboard.heading).toBeVisible();
  await expect(dashboard.youthCount).toHaveText('1');
  await expect(dashboard.pendingReviews).toContainText('2');
  await expect(dashboard.pendingReviews).toContainText('1 overdue');
  await expect(dashboard.averageProgress).toContainText('%');
  await expect(dashboard.atRiskCount).toHaveText('1');
  await expect(dashboard.rosterPreview).toContainText(seeded.youthName);
  await expect(dashboard.rosterPreview).toContainText('At risk');

  const review = dashboard.reviewItems.filter({ hasText: seeded.mentorReviewTitle });
  await expect(review).toContainText(seeded.youthName);
  await expect(review).toContainText('Assignment');
  await expect(review).toContainText('Overdue');
  await expect(review.getByRole('link', { name: /review/i })).toHaveAttribute(
    'href',
    `/mentor/reviews/submissions/${seeded.mentorReviewSubmissionId}`,
  );

  await dashboard.viewAllYouth.click();
  const roster = new MentorRosterPage(page);
  await expect(roster.heading).toBeVisible();
  await expect(roster.rows).toHaveCount(1);
  await expect(roster.rows).toContainText(seeded.youthName);

  await roster.search.fill('No such youth');
  await expect(roster.noMatches).toBeVisible();
  await roster.clearFilters.click();
  await expect(roster.rows).toHaveCount(1);
  await roster.filter('On track').click();
  await expect(roster.noMatches).toBeVisible();
  await roster.filter('At risk').click();
  await expect(roster.rows).toContainText(seeded.youthName);
});

test('mentor without assignments receives labelled empty states', async ({ page, request }) => {
  const mentor = await registerUser(request, 'Mentor', 'mentor-empty');
  await new SignInPage(page).signIn(mentor);

  const dashboard = new MentorDashboardPage(page);
  await expect(dashboard.noYouth).toBeVisible();
  await expect(dashboard.noReviews).toBeVisible();
  await expect(dashboard.youthCount).toHaveText('0');

  const roster = new MentorRosterPage(page);
  await roster.goto();
  await expect(roster.noMatches).toBeVisible();
  await expect(roster.noMatches).toContainText('No youth match');
});
