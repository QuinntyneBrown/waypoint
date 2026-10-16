import { expect, test } from '@playwright/test';
import { MentorDashboardPage, MentorRosterPage, SignInPage } from '../e2e/pages';
import { registerUser } from '../e2e/support/api';
import { rename, seedCast } from './mentoring-cast';
import { shot } from './shots';

const FOLDER = '056-mentoring-manage-mentor-roster';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: manage mentor roster', async ({ page, request, browser }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });

  const dashboard = new MentorDashboardPage(page);
  await expect(dashboard.youthCount).toHaveText('1');
  await expect(dashboard.rosterPreview).toContainText('David Osei');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'dashboard', { ...MAIN, height: 480 });

  await dashboard.viewAllYouth.click();
  const roster = new MentorRosterPage(page);
  await expect(roster.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'roster', { ...MAIN, height: 340 });

  await roster.search.fill('No such youth');
  await expect(roster.noMatches).toBeVisible();
  await shot(page, FOLDER, 'no-match', { ...MAIN, height: 600 });

  await roster.clearFilters.click();
  await roster.filter('At risk').click();
  await expect(roster.rows).toContainText('David Osei');
  await shot(page, FOLDER, 'at-risk', { ...MAIN, height: 340 });

  const mentor = await registerUser(request, 'Mentor', 'mentor-empty');
  await rename(request, mentor, 'Kofi Mensah');
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(fresh).signIn(mentor);
  const empty = new MentorDashboardPage(fresh);
  await expect(empty.noYouth).toBeVisible();
  await expect(empty.youthCount).toHaveText('0');
  await fresh.waitForLoadState('networkidle');
  await shot(fresh, FOLDER, 'empty', { ...MAIN, height: 640 });
});
