import { expect, test } from '@playwright/test';
import { DonatePage } from '../e2e/pages';
import { clipOf } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '018-public-site-start-donation';

test('capture: start a donation', async ({ page }) => {
  const donate = new DonatePage(page);
  await donate.goto();
  await expect(donate.continueButton).toBeVisible();
  await shot(page, FOLDER, 'form', { x: 300, y: 70, width: 680, height: 590 });

  await donate.email.fill('not-an-email');
  await donate.tier('$50/mo').click();
  await donate.continueButton.click();
  await expect(donate.nameError).toBeVisible();
  await expect(donate.emailError).toBeVisible();
  await shot(page, FOLDER, 'errors', { x: 300, y: 70, width: 680, height: 590 });

  await donate.donorName.fill('Sam Whitfield');
  await donate.email.fill('sam.whitfield@example.com');
  await donate.continueButton.click();
  await expect(donate.thankYou).toBeVisible();
  await expect(donate.providerLink).toBeVisible();
  const thanks = await clipOf(page.locator('.ph-alert--success'), 16, 90);
  await shot(page, FOLDER, 'thanks', { ...thanks, height: thanks.height + 90 });
});
