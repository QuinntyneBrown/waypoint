import { expect, test } from '@playwright/test';
import { GetInvolvedPage } from '../e2e/pages';
import { clipOf } from './public-site-demo';
import { shot } from './shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = '017-public-site-register-volunteer-interest';

test('capture: register volunteer interest', async ({ page }) => {
  const volunteer = new GetInvolvedPage(page);
  await volunteer.goto();
  await expect(volunteer.submitButton).toBeVisible();
  await shot(page, FOLDER, 'intro', { x: 0, y: 70, width: 1280, height: 560 });

  await volunteer.name.fill('Sam Whitfield');
  await volunteer.email.fill('sam.whitfield@example.com');
  await volunteer.areasOfInterest.fill('Tutoring in the coding track on weekday evenings.');
  await volunteer.submitButton.click();
  await expect(volunteer.screeningError).toBeVisible();
  await shot(page, FOLDER, 'blocked', await clipOf(page.locator('form'), 16, 90));

  await volunteer.acknowledgeScreening();
  await expect(volunteer.screeningCheckbox).toBeChecked();
  await shot(page, FOLDER, 'checked', await clipOf(page.locator('form'), 16, 90));

  await volunteer.submitButton.click();
  await expect(volunteer.acknowledgementHeading).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'thanks', await clipOf(page.locator('.w-empty'), 40));
});
