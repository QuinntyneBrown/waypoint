import { expect, test } from '@playwright/test';
import { SignInPage, YouthNotificationsPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ENGAGE-008, L2-NFR-005
test('a youth receives one live notification and follows its deep link', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const notifications = new YouthNotificationsPage(page);
  await notifications.goto();
  await expect(notifications.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });

  const mentor = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const reply = await request.post('/api/messages', {
    headers: { Authorization: `Bearer ${mentor.accessToken}` },
    data: { toUserId: seeded.youthId, body: 'Your next check-in is ready.' },
  });
  expect(reply.ok(), `mentor message failed: ${reply.status()} ${await reply.text()}`).toBeTruthy();

  const liveItem = notifications.notification('New message');
  await expect(liveItem).toContainText('You have a new message.', { timeout: 15_000 });
  await expect(liveItem).toHaveCount(1);
  await expect(liveItem).toHaveAttribute('href', '/youth/messages');

  await page.reload();
  await expect(notifications.notification('New message')).toHaveCount(1);
  await notifications.notification('New message').click();
  await expect(page).toHaveURL(/\/youth\/messages/);
  await expect(page.getByTestId('message-thread')).toContainText('Your next check-in is ready.');
});
