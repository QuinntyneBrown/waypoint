import { expect, test } from '@playwright/test';
import { AppShell, PortalAppPage, SignInPage } from '../pages';
import { registerYouth } from '../support/api';
import { contrastRatio } from '../support/contrast';

const THEMES = [
  {
    scheme: 'light',
    canvas: 'rgb(246, 247, 249)',
    text: 'rgb(26, 30, 38)',
    primary: { background: 'rgb(124, 58, 237)', color: 'rgb(255, 255, 255)' },
  },
  {
    scheme: 'dark',
    canvas: 'rgb(15, 18, 22)',
    text: 'rgb(233, 236, 241)',
    primary: { background: 'rgb(196, 181, 253)', color: 'rgb(46, 16, 101)' },
  },
] as const;

for (const theme of THEMES) {
  // Traces to: L2-NFR-012 AC1/AC2/AC5
  test(`pages render with the PromiseHub ${theme.scheme} tokens at AA contrast`, async ({
    page,
  }) => {
    await page.emulateMedia({ colorScheme: theme.scheme });
    const signIn = new SignInPage(page);
    await signIn.goto();
    await expect(signIn.submitButton).toBeVisible();

    const body = await signIn.bodyText();
    const primary = await signIn.primaryActionColors();
    expect(await signIn.canvasBackground()).toBe(theme.canvas);
    expect(body.color).toBe(theme.text);
    expect(body.fontFamily).toMatch(/^ui-rounded/);
    expect(primary).toEqual(theme.primary);
    expect(contrastRatio(body.color, theme.canvas)).toBeGreaterThanOrEqual(4.5);
    expect(contrastRatio(primary.color, primary.background)).toBeGreaterThanOrEqual(4.5);
  });
}

// Traces to: L2-NFR-012 AC1
test('fields and alerts take the PromiseHub component styles', async ({ page }) => {
  await page.emulateMedia({ colorScheme: 'light' });
  const signIn = new SignInPage(page);
  await signIn.goto();

  expect(await signIn.focusedFieldOutline()).toEqual({
    width: '2px',
    style: 'solid',
    color: 'rgb(124, 58, 237)',
  });
  await signIn.submitCredentials('nobody@example.com', 'not-the-password');
  await expect(signIn.errorAlert).toBeVisible();
  expect(await signIn.errorAlertFrame()).toEqual({ leftRule: '4px', radius: '12px' });
});

// Traces to: L2-NFR-012 AC3/AC4
test('the app root, shell and library components use the ph prefix', async ({ page, request }) => {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await expect(signIn.primaryAction).toBeVisible();

  await signIn.signIn(await registerYouth(request, 'ph-prefix'));
  await expect(new PortalAppPage(page).root).toBeAttached();
  await expect(new AppShell(page).host).toBeAttached();
});
