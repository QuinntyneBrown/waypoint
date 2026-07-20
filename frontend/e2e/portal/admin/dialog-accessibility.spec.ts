import { expect, test } from '@playwright/test';
import { CohortsPage, CurriculumBuilderPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-NFR-010 AC4
test('modal focus is trapped, Escape closes, and focus returns to its opener', async ({
  page,
  request,
}) => {
  await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'dialog-keyboard');
  await new SignInPage(page).signIn(admin);

  const cohorts = new CohortsPage(page);
  await cohorts.goto();
  const opener = cohorts.createButton;
  await opener.click();

  const dialog = cohorts.createDialog;
  const first = dialog.getByRole('button', { name: 'Close' });
  const last = dialog.getByRole('button', { name: 'Create cohort' });
  await expect(first).toBeFocused();

  await last.focus();
  await page.keyboard.press('Tab');
  await expect(first).toBeFocused();
  await page.keyboard.press('Shift+Tab');
  await expect(last).toBeFocused();

  await page.keyboard.press('Escape');
  await expect(dialog).toBeHidden();
  await expect(opener).toBeFocused();

  await opener.click();
  await page.locator('.wu-modal__scrim').click({ position: { x: 2, y: 2 } });
  await expect(dialog).toBeHidden();
  await expect(opener).toBeFocused();
});

// Traces to: L2-NFR-010 AC4
test('panel-style dialogs share keyboard management and retain backdrop close', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'panel-dialog-keyboard');
  await new SignInPage(page).signIn(admin);

  const curriculum = new CurriculumBuilderPage(page);
  await curriculum.goto();
  const opener = curriculum.newTrack;
  await opener.click();

  const dialog = curriculum.trackDialog;
  const first = dialog.getByLabel('Track name');
  const last = dialog.getByRole('button', { name: 'Cancel' });
  await expect(first).toBeFocused();

  await first.focus();
  await page.keyboard.press('Shift+Tab');
  await expect(last).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(first).toBeFocused();

  await page.keyboard.press('Escape');
  await expect(dialog).toBeHidden();
  await expect(opener).toBeFocused();

  await opener.click();
  await page.locator('.dialog-backdrop').click({ position: { x: 2, y: 2 } });
  await expect(dialog).toBeHidden();
  await expect(opener).toBeFocused();
});
