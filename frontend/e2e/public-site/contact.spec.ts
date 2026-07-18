import { expect, test } from '@playwright/test';
import { ContactPage } from '../pages';

// Traces to: L2-PUB-003
test('a visitor submits the contact form and sees an acknowledgement', async ({ page }) => {
  const contact = new ContactPage(page);
  await contact.goto();

  await contact.name.fill('Jordan Visitor');
  await contact.email.fill('jordan@example.com');
  await contact.topic.selectOption('Partnership');
  await contact.message.fill('We would love to partner with your program.');
  await contact.sendButton.click();

  await expect(contact.acknowledgementHeading).toBeVisible();
});

// Traces to: L2-PUB-003
test('missing fields and a malformed email block submission with field-level errors', async ({
  page,
}) => {
  const contact = new ContactPage(page);
  await contact.goto();

  // Leave name and message empty; enter a malformed email; then submit.
  await contact.email.fill('not-an-email');
  await contact.sendButton.click();

  await expect(contact.nameError).toBeVisible();
  await expect(contact.emailError).toBeVisible();
  await expect(contact.messageError).toBeVisible();
  await expect(contact.acknowledgementHeading).toHaveCount(0);
});
