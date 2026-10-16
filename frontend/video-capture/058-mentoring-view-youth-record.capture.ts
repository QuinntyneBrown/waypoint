import { expect, test } from '@playwright/test';
import { MentorYouthDetailPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';

const FOLDER = '058-mentoring-view-youth-record';
const MAIN = { x: 250, y: 70, width: 1030, height: 540 };

test('capture: view youth record', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto(`/mentor/youth/${seeded.youthId}`);

  const detail = new MentorYouthDetailPage(page);
  await expect(detail.heading).toHaveText('David Osei');
  await expect(detail.atRiskBanner).toContainText('Grace Adeyemi');
  await expect(detail.panel('Profile')).toContainText(seeded.guardianEmail);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'profile', MAIN);

  for (const tab of [
    'Progress',
    'Attendance',
    'Check-ins',
    'Submissions',
    'Competencies',
  ] as const) {
    await detail.tab(tab).click();
    await expect(detail.panel(tab)).toBeVisible();
    await page.waitForLoadState('networkidle');
    await shot(page, FOLDER, tab.toLowerCase(), MAIN);
  }
  await expect(page.getByText('PRIVATE PRAYER DETAIL')).toHaveCount(0);
});
