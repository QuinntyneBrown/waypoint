import { expect, test } from '@playwright/test';
import { SignInPage } from '../pages';
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
