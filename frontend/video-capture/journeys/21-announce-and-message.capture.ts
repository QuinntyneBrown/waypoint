import { expect, Locator, test } from '@playwright/test';
import {
  AdminAnnouncementsPage,
  MentorMessagesPage,
  SignInPage,
  YouthMessagesPage,
} from '../../e2e/pages';
import { seedEngagementCast, seedPat } from '../engagement-cast';
import { shotFull } from '../shotOf';
import { shot } from '../shots';

const FOLDER = 'journeys/21-announce-and-message';
const YOUTH = { x: 395, y: 90, width: 735, height: 220 };
const THREAD = { x: 390, y: 90, width: 750, height: 602 };
const MENTOR = { x: 250, y: 80, width: 1020, height: 700 };
const DIALOG = { x: 420, y: 240, width: 440, height: 330 };

test.setTimeout(120_000);
test('capture: announce and message journey', async ({ page, browser, request }) => {
  const cardShot = async (card: Locator, name: string) => {
    await card.scrollIntoViewIfNeeded();
    await page.evaluate(() => window.scrollBy(0, -150));
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

  const sentTitle = `Youth night ${crypto.randomUUID().slice(0, 4)}`;
  await announcements.title.fill(sentTitle);
  await announcements.message.fill('Youth night starts at 6 p.m. in the main studio.');
  await announcements.audience.selectOption('cohort');
  await announcements.cohort.selectOption(cast.cohortId);
  await announcements.recipients.selectOption('Youth');
  await announcements.sendNow.check();
  const form = page
    .locator('section.ph-card')
    .filter({ has: page.locator('#compose-announcement-heading') });
  await form.scrollIntoViewIfNeeded();
  await page.evaluate(() => window.scrollTo(0, 0));
  const formBox = (await form.boundingBox())!;
  await shotFull(page, FOLDER, 'compose', {
    x: formBox.x - 4,
    y: formBox.y,
    width: formBox.width + 8,
    height: formBox.height + 10,
  });
  await announcements.submit.click();
  await expect(announcements.status).toContainText('sent');
  await expect(announcements.announcement(sentTitle)).toContainText('0 of 1 read');
  await cardShot(announcements.announcement(sentTitle), 'sent-unread');

  const youthContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const youthPage = await youthContext.newPage();
  await new SignInPage(youthPage).signIn({ email: cast.youthEmail, password: cast.youthPassword });
  await youthPage.goto('/youth/announcements');
  const card = youthPage.locator('.ph-card').filter({ hasText: sentTitle });
  await expect(card).toContainText('New');
  await shot(youthPage, FOLDER, 'youth-new', YOUTH);

  await youthPage.goto('/youth/messages');
  const youthMessages = new YouthMessagesPage(youthPage);
  await expect(youthMessages.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });
  await youthMessages.composer.fill('I finished today’s lesson.');
  await youthMessages.sendButton.click();
  await expect(youthMessages.thread).toContainText('I finished today’s lesson.');

  const mentorContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const mentorPage = await mentorContext.newPage();
  await new SignInPage(mentorPage).signIn({
    email: cast.mentorEmail,
    password: cast.mentorPassword,
  });
  const mentor = new MentorMessagesPage(mentorPage);
  await mentor.goto();
  await expect(mentor.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });
  await expect(mentor.thread).toContainText('I finished today’s lesson.');
  await mentor.composer.fill('Great work. See you at the next session.');
  await mentor.sendButton.click();
  await expect(youthMessages.thread).toContainText('Great work. See you at the next session.', {
    timeout: 15_000,
  });
  await shot(mentorPage, FOLDER, 'mentor-thread', MENTOR);

  await mentor.broadcastButton.click();
  await mentor.broadcastBody.fill('Reminder: bring your journal on Monday.');
  await mentor.reviewBroadcast.click();
  const dialog = mentorPage.getByRole('dialog', { name: /send this broadcast/i });
  await expect(dialog).toBeVisible();
  await mentorPage.waitForTimeout(800);
  await shot(mentorPage, FOLDER, 'confirm', DIALOG);
  await mentor.confirmBroadcast.click();
  await expect(youthMessages.thread).toContainText('Reminder: bring your journal on Monday.', {
    timeout: 15_000,
  });
  await shot(youthPage, FOLDER, 'youth-thread', THREAD);

  await youthContext.close();
  await mentorContext.close();
});
