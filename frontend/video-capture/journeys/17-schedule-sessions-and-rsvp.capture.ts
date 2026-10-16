import { expect, test } from '@playwright/test';
import { AdminCalendarPage, SignInPage, YouthSchedulePage } from '../../e2e/pages';
import { seedEngagementCast, seedPat } from '../engagement-cast';
import { shotFull } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/17-schedule-sessions-and-rsvp';
const MAIN = { x: 255, y: 215, width: 700, height: 585 };
const CARD = { x: 255, y: 210, width: 1015, height: 530 };

test.setTimeout(120_000);
test('capture: schedule sessions and rsvp journey', async ({ page, browser, request }) => {
  const cast = await seedEngagementCast(request);
  const pat = await seedPat(request);
  await new SignInPage(page).signIn(pat);
  const calendar = new AdminCalendarPage(page);
  await calendar.goto();
  await expect(calendar.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  const formCard = page.locator('aside.session-form');

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
  await page.evaluate(() => window.scrollTo(0, 0));
  const box = (await formCard.boundingBox())!;
  await shotFull(page, FOLDER, 'form', {
    x: box.x - 8,
    y: box.y - 8,
    width: box.width + 16,
    height: box.height + 16,
  });

  await calendar.schedule.click();
  await expect(calendar.status).toContainText(/weekly sessions scheduled through the cohort term/i);
  await calendar.filter.selectOption(cast.cohortId);
  await calendar.agendaTab.click();
  await expect(calendar.session(title).first()).toContainText('Program Studio');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'scheduled', MAIN);

  const youthContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const youthPage = await youthContext.newPage();
  await new SignInPage(youthPage).signIn({ email: cast.youthEmail, password: cast.youthPassword });
  await youthPage.goto('/youth/schedule');
  const schedule = new YouthSchedulePage(youthPage);
  await expect(schedule.sessions.first()).toBeVisible();
  await youthPage.waitForLoadState('networkidle');
  await shot(youthPage, FOLDER, 'schedule', CARD);

  await schedule.maybeOption.first().click();
  await schedule.mentorNote.first().fill('I may arrive a few minutes late.');
  await schedule.reminderOption.first().click();
  await schedule.saveButton.first().click();
  await expect(schedule.status.first()).toContainText('RSVP saved.');
  await shot(youthPage, FOLDER, 'saved', CARD);
  await youthContext.close();
});
