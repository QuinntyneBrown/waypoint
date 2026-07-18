import { expect, test } from '@playwright/test';
import { GetInvolvedPage } from '../pages';

// Traces to: L2-PUB-003
test('a volunteer signs up after acknowledging the screening check and is thanked', async ({
  page,
}) => {
  const getInvolved = new GetInvolvedPage(page);
  await getInvolved.goto();

  await getInvolved.name.fill('Sam Helper');
  await getInvolved.email.fill('sam@example.com');
  await getInvolved.phone.fill('416-555-0100');
  await getInvolved.areasOfInterest.fill('Mentoring and tutoring.');
  await getInvolved.acknowledgeScreening();
  await expect(getInvolved.screeningCheckbox).toBeChecked();

  await getInvolved.submitButton.click();

  await expect(getInvolved.acknowledgementHeading).toBeVisible();
});

// Traces to: L2-PUB-003
test('volunteer submission is blocked until the screening check is acknowledged', async ({
  page,
}) => {
  const getInvolved = new GetInvolvedPage(page);
  await getInvolved.goto();

  await getInvolved.name.fill('Sam Helper');
  await getInvolved.email.fill('sam@example.com');
  // Leave the screening acknowledgement unchecked.
  await getInvolved.submitButton.click();

  await expect(getInvolved.screeningError).toBeVisible();
  await expect(getInvolved.acknowledgementHeading).toHaveCount(0);
});
