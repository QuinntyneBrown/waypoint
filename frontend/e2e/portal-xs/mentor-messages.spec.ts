import { expect, test } from '@playwright/test';
import { MentorMessagesPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-007, L2-NFR-009, L2-NFR-010
test('mentor messaging stays contained and touch/keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  const messages = new MentorMessagesPage(page);
  await messages.goto();

  await expect(messages.thread).toContainText('Welcome! Message me whenever you need support.');
  expect(await messages.horizontalOverflow()).toBeLessThanOrEqual(1);
  expect((await messages.sendButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await messages.composer.focus();
  await page.keyboard.type('Keyboard message from mobile.');
  await page.keyboard.press('Control+Enter');
  await expect(messages.thread).toContainText('Keyboard message from mobile.');

  await page.getByRole('button', { name: /back to inbox/i }).click();
  await expect(messages.broadcastButton).toBeVisible();
  expect((await messages.broadcastButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await messages.broadcastButton.click();
  await messages.broadcastBody.fill('Touch-ready cohort note.');
  await messages.reviewBroadcast.click();
  await expect(page.getByRole('dialog')).toContainText('1 youth');
  await messages.confirmBroadcast.click();
  await expect(page.getByRole('dialog')).toHaveCount(0);
  expect(await messages.horizontalOverflow()).toBeLessThanOrEqual(1);
});
