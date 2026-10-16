import { expect, test } from '@playwright/test';
import { AdminCalendarPage, SignInPage } from '../e2e/pages';
import { seedEngagementCast, seedPat } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '074-engagement-schedule-sessions';
const MAIN = { x: 255, y: 215, width: 700, height: 585 };

test.setTimeout(120_000);
test('capture: schedule sessions', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  const pat = await seedPat(request);
  await new SignInPage(page).signIn(pat);
  const calendar = new AdminCalendarPage(page);
  await calendar.goto();
  await expect(calendar.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  const formCard = page.locator('aside.session-form');
  await formCard.scrollIntoViewIfNeeded();
  const empty = (await formCard.boundingBox())!;
  await shot(page, FOLDER, 'empty-form', {
    x: empty.x - 10,
    y: Math.max(60, empty.y - 10),
    width: empty.width + 20,
    height: Math.min(empty.height + 20, 730),
  });

  const title = 'Weekly circle ' + crypto.randomUUID().slice(0, 4);
  const starts = new Date();
  starts.setDate(starts.getDate() + 2);
  await calendar.title.fill(title);
  await calendar.cohort.selectOption(cast.cohortId);
  await calendar.mentor.selectOption(cast.mentorId);
  await calendar.date.fill(starts.toISOString().slice(0, 10));
  await calendar.time.fill('17:30');
  await calendar.location.fill('Program Studio');
  await calendar.repeatsWeeklyControl.click();
  await expect(calendar.repeatsWeekly).toBeChecked();
  await formCard.scrollIntoViewIfNeeded();
  const filled = (await formCard.boundingBox())!;
  await shot(page, FOLDER, 'filled', {
    x: filled.x - 10,
    y: Math.max(60, filled.y - 10),
    width: filled.width + 20,
    height: Math.min(filled.height + 20, 730),
  });

  await calendar.schedule.click();
  await expect(calendar.status).toContainText(/weekly sessions scheduled through the cohort term/i);
  await calendar.filter.selectOption(cast.cohortId);
  await calendar.agendaTab.click();
  await expect(calendar.session(title).first()).toContainText('Program Studio');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'scheduled', MAIN);
});
