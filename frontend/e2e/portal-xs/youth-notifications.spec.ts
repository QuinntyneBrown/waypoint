import { expect, test } from '@playwright/test';
import { SignInPage, YouthNotificationsPage } from '../pages';
import { seedYouthCurriculum, signInApi } from '../support/api';

// Traces to: L2-ENGAGE-008, L2-NFR-005, L2-NFR-009, L2-NFR-010
test('live notification links remain contained and keyboard operable on mobile', async ({
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
    data: { toUserId: seeded.youthId, body: 'Open this update from your phone.' },
  });
  expect(reply.ok(), `mentor message failed: ${reply.status()} ${await reply.text()}`).toBeTruthy();

  const target = notifications.notification('New message');
  await expect(target).toBeVisible({ timeout: 15_000 });
  expect((await target.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  await target.focus();
  await page.keyboard.press('Enter');
  await expect(page).toHaveURL(/\/youth\/messages/);
});
