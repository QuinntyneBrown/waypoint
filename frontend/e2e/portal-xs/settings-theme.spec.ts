import { expect, test } from '@playwright/test';
import { registerYouth } from '../support/api';
import { DisplaySettingsPage, SignInPage } from '../pages';

// Traces to: L2-PLATFORM-013 AC6
test('every theme option is visible and operable on a phone without horizontal scroll', async ({
  page,
  request,
}) => {
  const creds = await registerYouth(request, 'theme-xs');
  await new SignInPage(page).signIn(creds);
  const display = new DisplaySettingsPage(page);
  await display.goto();

  for (const option of ['System', 'Light', 'Dark'] as const) {
    await expect(display.themeOption(option)).toBeInViewport();
  }
  await display.selectTheme('Dark');
  await expect(display.themeOption('Dark')).toHaveAttribute('aria-pressed', 'true');
  expect(await display.horizontalOverflow()).toBeLessThanOrEqual(1);
});
