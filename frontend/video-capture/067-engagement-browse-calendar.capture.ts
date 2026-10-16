import { test } from '@playwright/test';
import { AdminCalendarPage, SignInPage } from '../e2e/pages';
import { seedEngagementCast, seedPat } from './engagement-cast';
import { shot } from './shots';

const MAIN = { x: 255, y: 210, width: 700, height: 560 };
const FOLDER = '067-engagement-browse-calendar';

test('capture: browse calendar', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  const pat = await seedPat(request);
  await new SignInPage(page).signIn(pat);
  const calendar = new AdminCalendarPage(page);
  await calendar.goto();
  await calendar.heading.waitFor();
  await page.waitForLoadState('networkidle');
  await calendar.filter.selectOption(cast.cohortId);
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'month', MAIN);
  await calendar.weekTab.click();
  await page.getByRole('button', { name: 'Next', exact: true }).click();
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'week', MAIN);
  await page.getByRole('button', { name: 'Today', exact: true }).click();
  await calendar.agendaTab.click();
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'agenda', MAIN);
  await shot(page, FOLDER, 'session', { x: 264, y: 416, width: 686, height: 186 });
});
