import { expect, test } from '@playwright/test';
import { registerUser } from '../../support/api';
import { AppShell, SignInPage } from '../../pages';

// Traces to: L2-NFR-010 AC3
test('keyboard users can skip the navigation to the main content of a signed-in page', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'skip-link');
  await new SignInPage(page).signIn(admin);
  const shell = new AppShell(page);

  await page.keyboard.press('Tab');
  await expect(shell.skipLink).toBeFocused();

  await page.keyboard.press('Enter');
  await expect(shell.main).toBeFocused();
});
