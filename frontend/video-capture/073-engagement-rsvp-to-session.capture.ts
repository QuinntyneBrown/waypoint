import { expect, test } from '@playwright/test';
import { SignInPage, YouthSchedulePage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '073-engagement-rsvp-to-session';
const CARD = { x: 255, y: 210, width: 1015, height: 530 };

test.setTimeout(120_000);
test('capture: rsvp to session', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await new SignInPage(page).signIn({ email: cast.youthEmail, password: cast.youthPassword });
  await page.goto('/youth/schedule');
  const schedule = new YouthSchedulePage(page);
  await expect(schedule.sessions).toHaveCount(1);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'schedule', CARD);

  await schedule.maybeOption.click();
  await schedule.mentorNote.fill('I may arrive a few minutes late.');
  await schedule.reminderOption.click();
  await shot(page, FOLDER, 'filled', CARD);

  await schedule.saveButton.click();
  await expect(schedule.status).toContainText('RSVP saved.');
  await expect(schedule.reminderTime).toBeVisible();
  await shot(page, FOLDER, 'saved', CARD);

  await schedule.cantMakeItOption.click();
  await expect(schedule.reminder).toBeDisabled();
  await schedule.saveButton.click();
  await expect(schedule.currentResponse).toContainText('Can’t make it');
  await shot(page, FOLDER, 'cant-make-it', CARD);
});
