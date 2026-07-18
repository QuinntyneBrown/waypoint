import { expect, test } from '@playwright/test';
import { ApplyPage } from '../pages';

// Traces to: L2-INTAKE-001
test('a minor applicant submits a valid application and sees confirmation (no account yet)', async ({
  page,
}) => {
  const apply = new ApplyPage(page);
  await apply.goto();

  await apply.firstName.fill('Sam');
  await apply.lastName.fill('Rivera');
  await apply.age.fill('15');

  // Under-18 → guardian fields appear and are required.
  await expect(apply.guardianName).toBeVisible();

  await apply.neighbourhood.fill('Rexdale');
  await apply.interest.fill('I want to grow as a leader and serve.');
  await apply.guardianName.fill('Pat Rivera');
  await apply.guardianEmail.fill('pat.rivera@example.com');
  await apply.guardianPhone.fill('416-555-0134');

  // The native checkbox is visually hidden behind a custom box; toggle via the label.
  await apply.acknowledgeParticipation();
  await apply.acknowledgePrivacy();
  await expect(apply.participationCheckbox).toBeChecked();
  await expect(apply.privacyCheckbox).toBeChecked();

  await apply.submitButton.click();

  await expect(apply.confirmationHeading).toBeVisible();
  await expect(apply.accountNotice).toBeVisible();
});

// Traces to: L2-INTAKE-001
test('an age outside 12–18 is blocked with the eligible-ages message', async ({ page }) => {
  const apply = new ApplyPage(page);
  await apply.goto();
  await apply.age.fill('20');
  await expect(apply.eligibleAgesError).toBeVisible();
});

// Traces to: L2-INTAKE-001
test('missing consent acknowledgements block submission with field errors', async ({ page }) => {
  const apply = new ApplyPage(page);
  await apply.goto();
  await apply.firstName.fill('Sam');
  await apply.lastName.fill('Rivera');
  await apply.age.fill('16');
  await apply.neighbourhood.fill('Rexdale');
  await apply.interest.fill('Growth');
  await apply.guardianName.fill('Pat Rivera');
  await apply.guardianEmail.fill('pat.rivera@example.com');
  await apply.guardianPhone.fill('416-555-0134');

  // Leave both acknowledgements unchecked.
  await apply.submitButton.click();

  await expect(apply.participationError).toBeVisible();
  await expect(apply.privacyError).toBeVisible();
  await expect(apply.confirmationHeading).toHaveCount(0);
});
