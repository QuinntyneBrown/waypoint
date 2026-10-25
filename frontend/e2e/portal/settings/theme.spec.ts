import { expect, test } from '@playwright/test';
import { registerYouth } from '../../support/api';
import { DisplaySettingsPage, SignInPage } from '../../pages';

const DARK_CANVAS = 'rgb(15, 18, 22)';
const LIGHT_CANVAS = 'rgb(246, 247, 249)';

// Traces to: L2-PLATFORM-013 AC1/AC5
test('signed-out pages follow a dark device colour scheme', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'dark' });
  const signIn = new SignInPage(page);
  await signIn.goto();

  expect(await signIn.canvasBackground()).toBe(DARK_CANVAS);
  expect(await signIn.colorScheme()).toBe('dark');
});

// Traces to: L2-PLATFORM-013 AC1/AC5
test('signed-out pages follow a light device colour scheme', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'light' });
  const signIn = new SignInPage(page);
  await signIn.goto();

  expect(await signIn.canvasBackground()).toBe(LIGHT_CANVAS);
  expect(await signIn.colorScheme()).toBe('light');
});

// Traces to: L2-PLATFORM-013 AC2
test('choosing a theme previews it immediately without saving', async ({ page, request }) => {
  const creds = await registerYouth(request, 'theme-preview');
  await page.emulateMedia({ colorScheme: 'light' });
  await new SignInPage(page).signIn(creds);
  const display = new DisplaySettingsPage(page);
  await display.goto();
  const saves: string[] = [];
  page.on('request', (r) => {
    if (r.method() === 'PUT' && r.url().includes('/api/me/display-preferences'))
      saves.push(r.url());
  });

  await display.selectTheme('Dark');
  await expect(display.themeOption('Dark')).toHaveAttribute('aria-pressed', 'true');
  expect(await display.rootTheme()).toBe('dark');
  expect(await display.canvasBackground()).toBe(DARK_CANVAS);

  await display.selectTheme('System');
  expect(await display.rootTheme()).toBeNull();
  expect(await display.canvasBackground()).toBe(LIGHT_CANVAS);
  expect(saves).toEqual([]);
});

// Traces to: L2-PLATFORM-013 AC3
test('a saved theme overrides the device colour scheme after reload', async ({ page, request }) => {
  const creds = await registerYouth(request, 'theme-save');
  await page.emulateMedia({ colorScheme: 'light' });
  await new SignInPage(page).signIn(creds);
  const display = new DisplaySettingsPage(page);
  await display.goto();

  await display.selectTheme('Dark');
  await display.save();
  await expect(display.savedToast).toBeVisible();

  await page.reload();
  await expect(display.themeOption('Dark')).toHaveAttribute('aria-pressed', 'true');
  expect(await display.rootTheme()).toBe('dark');
  expect(await display.canvasBackground()).toBe(DARK_CANVAS);
});
