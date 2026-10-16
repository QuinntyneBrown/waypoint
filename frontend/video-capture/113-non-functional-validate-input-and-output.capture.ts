import { expect, test } from '@playwright/test';
import { YouthMessagesPage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '113-non-functional-validate-input-and-output';
const MARKUP = `<b>Bold?</b> <script>alert('hello')</script>`;

test.setTimeout(240_000);

test('capture: field errors and text shown as plain text', async ({ page, request }) => {
  // images/errors-1280.png is the sign-in form with empty fields, taken by the 105 capture.
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.youthEmail, password: cast.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto('/youth/messages');
  const messages = new YouthMessagesPage(page);
  await expect(messages.thread).toContainText(cast.mentorName);
  await messages.composer.fill(MARKUP);
  await messages.sendButton.click();
  await expect(messages.thread).toContainText('<b>Bold?</b>');
  await expect(messages.thread).toContainText('<script>');
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'plain-text', { x: 244, y: 56, width: 1036, height: 600 });
});
