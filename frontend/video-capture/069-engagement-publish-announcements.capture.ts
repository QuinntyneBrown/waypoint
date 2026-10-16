import { expect, Locator, test } from '@playwright/test';
import { AdminAnnouncementsPage, SignInPage } from '../e2e/pages';
import { seedEngagementCast, seedPat } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '069-engagement-publish-announcements';
const YOUTH = { x: 395, y: 90, width: 735, height: 220 };

test.setTimeout(120_000);
test('capture: publish announcements', async ({ page, browser, request }) => {
  const cardShot = async (card: Locator, name: string) => {
    await card.scrollIntoViewIfNeeded();
    const box = (await card.boundingBox())!;
    await shot(page, FOLDER, name, {
      x: box.x - 10,
      y: box.y - 10,
      width: box.width + 20,
      height: box.height + 20,
    });
  };
  const cast = await seedEngagementCast(request);
  const pat = await seedPat(request);
  await new SignInPage(page).signIn(pat);
  const announcements = new AdminAnnouncementsPage(page);
  await announcements.goto();
  await expect(announcements.heading).toBeVisible();

  const scheduledTitle = `Trip details ${crypto.randomUUID().slice(0, 4)}`;
  await announcements.title.fill(scheduledTitle);
  await announcements.message.fill('Guardians will receive the final trip details tomorrow.');
  await announcements.audience.selectOption('org');
  await announcements.recipients.selectOption('Guardians');
  await announcements.scheduleLater.check();
  const tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000);
  await announcements.scheduledAt.fill(tomorrow.toISOString().slice(0, 16));
  const form = page
    .locator('section.w-card')
    .filter({ has: page.locator('#compose-announcement-heading') });
  await form.scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollTo(0, 0));
  const formBox = (await form.boundingBox())!;
  await shot(page, FOLDER, 'compose-top', {
    x: formBox.x - 10,
    y: formBox.y,
    width: formBox.width + 20,
    height: 580,
  });
  await announcements.submit.scrollIntoViewIfNeeded();
  const submitBox = (await announcements.submit.boundingBox())!;
  await shot(page, FOLDER, 'compose-delivery', {
    x: formBox.x - 10,
    y: Math.max(0, submitBox.y + submitBox.height + 30 - 700),
    width: formBox.width + 20,
    height: 700,
  });
  await announcements.submit.click();
  await expect(announcements.status).toContainText('scheduled');
  await cardShot(announcements.announcement(scheduledTitle), 'scheduled');

  const sentTitle = `Youth night ${crypto.randomUUID().slice(0, 4)}`;
  await announcements.title.fill(sentTitle);
  await announcements.message.fill('Youth night starts at 6 p.m. in the main studio.');
  await announcements.audience.selectOption('cohort');
  await announcements.cohort.selectOption(cast.cohortId);
  await announcements.recipients.selectOption('Youth');
  await announcements.sendNow.check();
  await announcements.submit.click();
  await expect(announcements.status).toContainText('sent');
  await expect(announcements.announcement(sentTitle)).toContainText('0 of 1 read');
  await cardShot(announcements.announcement(sentTitle), 'sent-unread');

  const youthContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const youthPage = await youthContext.newPage();
  await new SignInPage(youthPage).signIn({ email: cast.youthEmail, password: cast.youthPassword });
  await youthPage.goto('/youth/announcements');
  const card = youthPage.locator('.w-card').filter({ hasText: sentTitle });
  await expect(card).toContainText('New');
  await shot(youthPage, FOLDER, 'youth-new', YOUTH);
  await youthPage.getByRole('button', { name: 'Mark all read', exact: true }).click();
  await expect(card).not.toContainText('New');
  await shot(youthPage, FOLDER, 'youth-read', YOUTH);
  await youthContext.close();

  await announcements.goto();
  await expect(announcements.announcement(sentTitle)).toContainText('100% read');
  await cardShot(announcements.announcement(sentTitle), 'sent-read');
});
