import { expect, test } from '@playwright/test';
import { SignInPage, YouthMessagesPage, YouthNotificationsPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '104-non-functional-deliver-realtime-events';
const MAIN = { x: 244, y: 56, width: 1036, height: 600 };
const NOTE = { x: 244, y: 56, width: 1036, height: 330 };

test.setTimeout(240_000);

test('capture: live notification and live message', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.youthEmail, password: cast.youthPassword });

  const notifications = new YouthNotificationsPage(page);
  await notifications.goto();
  await expect(notifications.realtimeStatus).toContainText('Live updates on', { timeout: 30_000 });
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'waiting', NOTE);

  const mentor = await signInApi(request, {
    email: cast.mentorEmail,
    password: cast.mentorPassword,
  });
  const headers = { Authorization: `Bearer ${mentor.accessToken}` };
  const sent = await request.post('/api/messages', {
    headers,
    data: { toUserId: cast.youthId, body: 'Your next check-in is ready.' },
  });
  expect(sent.ok(), `mentor message failed: ${sent.status()} ${await sent.text()}`).toBeTruthy();

  const liveItem = notifications.notification('New message');
  await expect(liveItem).toContainText('You have a new message.', { timeout: 30_000 });
  await expect(liveItem).toHaveCount(1);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'arrived', NOTE);

  await page.goto('/youth/messages');
  const messages = new YouthMessagesPage(page);
  await expect(messages.thread).toContainText('Your next check-in is ready.');
  await expect(messages.realtimeStatus).toContainText('Live updates on', { timeout: 30_000 });
  const reply = await request.post('/api/messages', {
    headers,
    data: { toUserId: cast.youthId, body: 'I saw your lesson update. Well done.' },
  });
  expect(reply.ok()).toBeTruthy();
  await expect(messages.thread).toContainText('I saw your lesson update. Well done.', {
    timeout: 30_000,
  });
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'message', MAIN);
});
