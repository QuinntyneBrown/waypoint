import { expect, test } from '@playwright/test';
import {
  MentorDashboardPage,
  MentorRosterPage,
  MentorYouthDetailPage,
  SignInPage,
} from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/02-mentor-dashboard';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: mentor dashboard journey', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });

  const dashboard = new MentorDashboardPage(page);
  await expect(dashboard.youthCount).toHaveText('1');
  await expect(dashboard.rosterPreview).toContainText('David Osei');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'dashboard', { ...MAIN, height: 560 });

  await dashboard.viewAllYouth.click();
  const roster = new MentorRosterPage(page);
  await expect(roster.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'roster', { ...MAIN, height: 340 });

  await roster.filter('At risk').click();
  await expect(roster.rows).toContainText('David Osei');
  await shot(page, FOLDER, 'at-risk', { ...MAIN, height: 340 });

  await page.goto(`/mentor/youth/${seeded.youthId}`);
  const detail = new MentorYouthDetailPage(page);
  await expect(detail.heading).toHaveText('David Osei');
  await expect(detail.atRiskBanner).toContainText('Grace Adeyemi');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'record', { ...MAIN, height: 540 });

  await detail.tab('Attendance').click();
  await expect(detail.panel('Attendance')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'attendance', { ...MAIN, height: 540 });
});
