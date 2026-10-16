import { expect, test } from '@playwright/test';
import { MentorCheckInsPage, MentorYouthDetailPage, SignInPage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shotFull, shotOf } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/15-check-in';
const MAIN = { x: 250, y: 70, width: 1030, height: 540 };

test('capture: check-in journey', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });

  await page.goto(`/mentor/youth/${seeded.youthId}`);
  const detail = new MentorYouthDetailPage(page);
  await expect(detail.heading).toHaveText('Amara Bell');
  await detail.tab('Check-ins').click();
  await expect(detail.panel('Check-ins')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'record-tab', { x: 250, y: 300, width: 1030, height: 275 });

  await page.goto(`/mentor/check-ins?youthId=${seeded.youthId}`);
  const journal = new MentorCheckInsPage(page);
  await expect(journal.youth).toHaveValue(seeded.youthId);
  await expect(journal.share).not.toBeChecked();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'form', { x: 250, y: 70, width: 1030, height: 730 });

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
