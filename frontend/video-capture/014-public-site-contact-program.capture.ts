import { expect, test } from '@playwright/test';
import { ContactPage } from '../e2e/pages';
import { clipOf } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '014-public-site-contact-program';

test('capture: contact the program', async ({ page }) => {
  const contact = new ContactPage(page);
  await contact.goto();
  await expect(contact.sendButton).toBeVisible();
  await shot(page, FOLDER, 'page', { x: 300, y: 70, width: 680, height: 430 });
  await shot(page, FOLDER, 'form', await clipOf(page.locator('form'), 16, 90));

  await contact.email.fill('not-an-email');
  await contact.sendButton.click();
  await expect(contact.nameError).toBeVisible();
  await expect(contact.emailError).toBeVisible();
  await expect(contact.messageError).toBeVisible();
  await shot(page, FOLDER, 'errors', await clipOf(page.locator('form'), 16, 90));

  await contact.name.fill('Sam Whitfield');
  await contact.email.fill('sam.whitfield@example.com');
  await contact.topic.selectOption('Partnership');
  await contact.message.fill('Our church would like to talk about hosting a cohort evening.');
  await contact.sendButton.click();
  await expect(contact.acknowledgementHeading).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'sent', await clipOf(page.locator('.w-empty'), 40));
});
