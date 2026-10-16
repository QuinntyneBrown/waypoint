import { expect, test } from '@playwright/test';
import { MentorMessagesPage, SignInPage, YouthMessagesPage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { shot } from './shots';

const FOLDER = '068-engagement-exchange-messages';
const YOUTH = { x: 390, y: 90, width: 750, height: 640 };
const MENTOR = { x: 250, y: 80, width: 1020, height: 700 };
const DIALOG = { x: 420, y: 240, width: 440, height: 330 };

test.setTimeout(120_000);
test('capture: exchange messages', async ({ page, browser, request }) => {
  const cast = await seedEngagementCast(request);

  const youthContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const youthPage = await youthContext.newPage();
  await new SignInPage(youthPage).signIn({ email: cast.youthEmail, password: cast.youthPassword });
  await youthPage.goto('/youth/messages');
  const youthMessages = new YouthMessagesPage(youthPage);
  await expect(youthMessages.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });
  await youthMessages.composer.fill('I finished today’s lesson.');
  await youthMessages.sendButton.click();
  await expect(youthMessages.thread).toContainText('I finished today’s lesson.');
  await shot(youthPage, FOLDER, 'youth-thread', YOUTH);

  await new SignInPage(page).signIn({ email: cast.mentorEmail, password: cast.mentorPassword });
  const mentor = new MentorMessagesPage(page);
  await mentor.goto();
  await expect(mentor.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });
  await expect(mentor.thread).toContainText('I finished today’s lesson.');
  await mentor.composer.fill('Great work. See you at the next session.');
  await mentor.sendButton.click();
  await expect(mentor.thread).toContainText('Great work. See you at the next session.');
  await expect(youthMessages.thread).toContainText('Great work. See you at the next session.', {
    timeout: 15_000,
  });
  await shot(page, FOLDER, 'mentor-thread', MENTOR);
  await shot(youthPage, FOLDER, 'youth-reply', YOUTH);

  await mentor.broadcastButton.click();
  await expect(page.getByRole('heading', { name: /broadcast to my group/i })).toBeVisible();
  await mentor.broadcastBody.fill('Reminder: bring your journal on Monday.');
  await shot(page, FOLDER, 'broadcast', MENTOR);
  await mentor.reviewBroadcast.click();
  const dialog = page.getByRole('dialog', { name: /send this broadcast/i });
  await expect(dialog).toBeVisible();
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'confirm', DIALOG);
  await mentor.confirmBroadcast.click();
  await expect(youthMessages.thread).toContainText('Reminder: bring your journal on Monday.', {
    timeout: 15_000,
  });
  await shot(youthPage, FOLDER, 'youth-broadcast', YOUTH);
  await youthContext.close();
});
