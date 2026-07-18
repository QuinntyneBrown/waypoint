import { expect, test } from '@playwright/test';
import { MentorMessagesPage, SignInPage, YouthMessagesPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ENGAGE-007, L2-ENGAGE-008, L2-NFR-005, L2-AUTH-010
test.setTimeout(60_000);
test('mentor uses private threads and cohort broadcast with live delivery in both directions', async ({
  browser,
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });

  const mentorMessages = new MentorMessagesPage(page);
  await mentorMessages.goto();
  await expect(mentorMessages.heading).toBeVisible();
  await expect(mentorMessages.realtimeStatus).toContainText('Live updates on', {
    timeout: 15_000,
  });
  await expect(page.getByRole('heading', { name: seeded.youthName, exact: true })).toBeVisible();
  await expect(mentorMessages.thread).toContainText(
    'Welcome! Message me whenever you need support.',
  );

  await mentorMessages.composer.fill('You are ready for this week.');
  await mentorMessages.sendButton.click();
  await expect(mentorMessages.thread).toContainText('You are ready for this week.');

  const youthContext = await browser.newContext({
    baseURL: test.info().project.use.baseURL as string,
  });
  try {
    const youthPage = await youthContext.newPage();
    await new SignInPage(youthPage).signIn({
      email: seeded.youthEmail,
      password: seeded.youthPassword,
    });
    await youthPage.goto('/youth/messages');
    const youthMessages = new YouthMessagesPage(youthPage);
    await expect(youthMessages.realtimeStatus).toContainText('Live updates on', {
      timeout: 15_000,
    });
    await expect(youthMessages.thread).toContainText('You are ready for this week.');

    await mentorMessages.broadcastButton.click();
    await expect(page.getByRole('heading', { name: /broadcast to my group/i })).toBeVisible();
    await expect(page.getByLabel('Cohort')).toHaveValue(seeded.cohortId);
    await mentorMessages.broadcastBody.fill('Cohort reminder: bring your journal.');
    await mentorMessages.reviewBroadcast.click();
    const dialog = page.getByRole('dialog', { name: /send this broadcast/i });
    await expect(dialog).toContainText(seeded.cohortName);
    await expect(dialog).toContainText('1 youth');
    const broadcastResponse = page.waitForResponse(
      (response) =>
        response.url().endsWith('/api/messages/broadcast') &&
        response.request().method() === 'POST',
    );
    await mentorMessages.confirmBroadcast.click();
    expect((await broadcastResponse).ok()).toBeTruthy();
    await expect(youthMessages.thread).toContainText('Cohort reminder: bring your journal.', {
      timeout: 15_000,
    });
    await expect(
      youthMessages.thread.getByText('Cohort reminder: bring your journal.', { exact: true }),
    ).toHaveCount(1);

    await youthMessages.composer.fill('Thanks — I have it ready.');
    await youthMessages.sendButton.click();
    await mentorMessages.conversation(seeded.youthName).click();
    await expect(mentorMessages.thread).toContainText('Thanks — I have it ready.', {
      timeout: 15_000,
    });
    await expect(
      mentorMessages.thread.getByText('Thanks — I have it ready.', { exact: true }),
    ).toHaveCount(1);

    await page.goto('/mentor/notifications');
    await expect(page.getByRole('heading', { name: 'Notifications', exact: true })).toBeVisible();
    const messageNotification = page
      .getByTestId('notification-item')
      .filter({ hasText: 'New message' })
      .first();
    await expect(messageNotification.getByText('New', { exact: true })).toBeVisible();
    await expect(messageNotification).toHaveAttribute('href', '/mentor/messages');
    await page.getByRole('button', { name: /mark all read/i }).click();
    await expect(messageNotification.getByText('New', { exact: true })).toHaveCount(0);
  } finally {
    await youthContext.close();
  }
});
