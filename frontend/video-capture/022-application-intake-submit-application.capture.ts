import { expect, test } from '@playwright/test';
import { ApplyPage } from '../e2e/pages';
import { shot } from './shots';

const FOLDER = '022-application-intake-submit-application';
const SITE = 'http://localhost:4201';
const FORM = { x: 240, y: 70, width: 800, height: 600 };

test('capture: submit an application', async ({ page }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const apply = new ApplyPage(page);
  await apply.goto(SITE);
  await expect(apply.submitButton).toBeVisible();
  await shot(page, FOLDER, 'form', FORM);

  // Eligibility message for an age outside the range.
  await apply.age.fill('20');
  await expect(apply.eligibleAgesError).toBeVisible();
  await shot(page, FOLDER, 'age', FORM);

  // Under-18 shows the guardian section.
  await apply.age.fill('15');
  await expect(apply.guardianName).toBeVisible();
  await apply.guardianName.scrollIntoViewIfNeeded();
  await page.waitForTimeout(300);
  await shot(page, FOLDER, 'guardian', FORM);

  // Missing acknowledgements block submission with field errors.
  await apply.firstName.fill('Imani');
  await apply.lastName.fill('Clarke');
  await apply.neighbourhood.fill('Peckham');
  await apply.cohortInterest.fill('Peckham Autumn cohort');
  await apply.interest.fill('I want to grow as a leader and serve.');
  await apply.guardianName.fill('Ruth Clarke');
  await apply.guardianEmail.fill('ruth.clarke@example.com');
  await apply.guardianPhone.fill('416-555-0134');
  await apply.submitButton.click();
  await expect(apply.participationError).toBeVisible();
  await expect(apply.privacyError).toBeVisible();
  await apply.participationError.scrollIntoViewIfNeeded();
  await page.waitForTimeout(300);
  await shot(page, FOLDER, 'acks', FORM);

  await apply.acknowledgeParticipation();
  await apply.acknowledgePrivacy();
  await apply.submitButton.click();
  await expect(apply.confirmationHeading).toBeVisible();
  await page.waitForTimeout(300);
  await shot(page, FOLDER, 'confirmation', { x: 240, y: 100, width: 800, height: 360 });
});
