import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { SignInPage, SupportPage } from '../../pages';

// Traces to: L2-PLATFORM-005
test('a support request is submitted and acknowledged', async ({ page, request }) => {
  const creds = await registerYouth(request, 'support');
  await new SignInPage(page).signIn(creds);

  const support = new SupportPage(page);
  await support.goto();

  await support.message.fill('I can’t find my lessons anywhere.');
  // Category defaults to the first option; reply-to email is prefilled from the session.
  await support.sendButton.click();

  await expect(support.acknowledgementHeading).toBeVisible();
});

// Traces to: L2-PLATFORM-005 AC2, L2-NFR-001 AC4
test('an optional support screenshot is sniffed, removable, and linked to the request', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'support-screenshot');
  await new SignInPage(page).signIn(creds);

  const support = new SupportPage(page);
  await support.goto();
  await support.screenshotInput.setInputFiles({
    name: 'problem.png',
    mimeType: 'image/png',
    buffer: Buffer.from('<script>alert(1)</script>'),
  });
  await expect(page.getByRole('alert')).toContainText(/valid jpg or png/i);

  const screenshot = {
    name: 'problem.png',
    mimeType: 'image/png',
    buffer: Buffer.from(
      'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      'base64',
    ),
  };
  await support.screenshotInput.setInputFiles(screenshot);
  await expect(page.getByRole('status')).toContainText(/screenshot ready: problem\.png/i);
  await expect(page.getByRole('img', { name: /support screenshot preview/i })).toBeVisible();
  await support.removeScreenshotButton.click();
  await expect(page.getByRole('status')).toContainText(/screenshot removed/i);

  await support.screenshotInput.setInputFiles(screenshot);
  await expect(page.getByRole('status')).toContainText(/screenshot ready/i);
  await support.message.fill('The screenshot shows the lesson page failing to load.');
  await support.sendButton.click();
  await expect(support.acknowledgementHeading).toBeVisible();
});
