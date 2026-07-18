import { expect, test } from '@playwright/test';
import { SignInPage, YouthMessagesPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-007, L2-NFR-009, L2-NFR-010
test('mentor thread remains contained and keyboard operable on mobile', async ({
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
  await expect(messages.thread).toContainText('Welcome! Message me whenever you need support.');
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await messages.sendButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await messages.composer.focus();
  await page.keyboard.type('Sent entirely with the keyboard.');
  await page.keyboard.press('Control+Enter');
  await expect(messages.thread).toContainText('Sent entirely with the keyboard.');
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
