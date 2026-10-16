import { expect, test } from '@playwright/test';
import { MentorCheckInsPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';
import { shotFull, shotOf } from './shotOf';

const FOLDER = '057-mentoring-record-check-ins';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: record check-ins', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto(`/mentor/check-ins?youthId=${seeded.youthId}`);

  const journal = new MentorCheckInsPage(page);
  await expect(journal.youth).toHaveValue(seeded.youthId);
  await expect(journal.share).not.toBeChecked();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'form', MAIN);

  await journal.type.selectOption('call');
  await journal.engagement.fill('Hopeful and ready to lead.');
  await journal.goals.fill('Welcome the next new participant.');
  await journal.prayer.fill('Prayer for courage.');
  await journal.actions.fill('Draft a welcome message.');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shotFull(page, FOLDER, 'filled', { x: 250, y: 205, width: 400, height: 790 });

  await journal.shareControl.click();
  await expect(page.getByText(/including prayer requests/i)).toBeVisible();
  await journal.guardianInformedControl.click();
  await journal.save.scrollIntoViewIfNeeded();
  await page.waitForTimeout(300);
  await shot(page, FOLDER, 'share', { x: 250, y: 56, width: 400, height: 744 });

  await journal.save.click();
  await expect(journal.status).toContainText('Check-in saved');
  await expect(journal.historyItems.filter({ hasText: 'Prayer for courage.' })).toHaveCount(1);
  await shotOf(page, FOLDER, 'history', journal.historyItems.first(), 12);
});
