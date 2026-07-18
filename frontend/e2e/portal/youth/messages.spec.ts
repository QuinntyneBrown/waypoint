import { expect, test } from '@playwright/test';
import { SignInPage, YouthMessagesPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ENGAGE-007, L2-NFR-005
test('youth exchanges messages with the assigned mentor and receives a live reply', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto('/youth/messages');

  const messages = new YouthMessagesPage(page);
  await expect(messages.heading).toBeVisible();
  await expect(messages.thread).toContainText(seeded.mentorName);
  await expect(messages.thread).toContainText('Welcome! Message me whenever you need support.');

  await messages.composer.fill('I finished today’s lesson.');
  await messages.sendButton.click();
  await expect(messages.thread).toContainText('I finished today’s lesson.');

  await expect(messages.realtimeStatus).toContainText('Live updates on', { timeout: 15_000 });
  const mentor = await signInApi(request, {
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const reply = await request.post('/api/messages', {
    headers: { Authorization: `Bearer ${mentor.accessToken}` },
    data: { toUserId: seeded.youthId, body: 'Great work — I saw your update live.' },
  });
  expect(reply.ok(), `mentor reply failed: ${reply.status()} ${await reply.text()}`).toBeTruthy();
  await expect(messages.thread).toContainText('Great work — I saw your update live.', {
    timeout: 15_000,
  });

  const matchingLiveReplies = messages.thread.getByText('Great work — I saw your update live.', {
    exact: true,
  });
  await expect(matchingLiveReplies).toHaveCount(1);
});
