import { expect, test } from '@playwright/test';
import { NotificationsSettingsPage, SignInPage, YouthNotificationsPage } from '../../e2e/pages';
import { signInApi } from '../../e2e/support/api';
import { seedEngagementCast } from '../engagement-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/22-receive-notifications';
const FEED = { x: 395, y: 90, width: 735, height: 310 };
const PAGE = { x: 395, y: 90, width: 735, height: 700 };

test.setTimeout(120_000);
test('capture: receive notifications journey', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await new SignInPage(page).signIn({ email: cast.youthEmail, password: cast.youthPassword });

  const notifications = new YouthNotificationsPage(page);
  await notifications.goto();
  await expect(notifications.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });

  const mentor = await signInApi(request, {
    email: cast.mentorEmail,
    password: cast.mentorPassword,
  });
  const sent = await request.post('/api/messages', {
    headers: { Authorization: `Bearer ${mentor.accessToken}` },
    data: { toUserId: cast.youthId, body: 'Your next check-in is ready.' },
  });
  expect(sent.ok()).toBeTruthy();
  const item = notifications.notification('New message');
  await expect(item).toContainText('You have a new message.', { timeout: 15_000 });
  await shot(page, FOLDER, 'live', FEED);

  await page.getByRole('button', { name: /mark all read/i }).click();
  await expect(item.getByText('New', { exact: true })).toHaveCount(0);
  await shot(page, FOLDER, 'read', FEED);

  await item.click();
  await expect(page).toHaveURL(/\/youth\/messages/);
  await expect(page.getByTestId('message-thread')).toContainText('Your next check-in is ready.');
  await shot(page, FOLDER, 'deep-link', PAGE);

  const prefs = new NotificationsSettingsPage(page);
  await prefs.goto();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'preferences', PAGE);
});
