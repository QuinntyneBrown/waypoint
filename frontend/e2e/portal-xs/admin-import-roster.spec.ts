import { expect, test } from '@playwright/test';
import { AdminRosterImportPage, SignInPage } from '../pages';
import { registerUser } from '../support/api';

// Traces to: L2-REPORT-004, L2-NFR-009, L2-NFR-010
test('roster upload and mapping remain keyboard and touch usable on mobile', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'roster-import-mobile');
  await new SignInPage(page).signIn(admin);
  const roster = new AdminRosterImportPage(page);
  await roster.goto();
  await roster.fileInput.setInputFiles({
    name: 'mobile.csv',
    mimeType: 'text/csv',
    buffer: Buffer.from('Name,Email\nMobile Youth,mobile.youth@example.com'),
  });

  await roster.mapColumn('Name').focus();
  await page.keyboard.press('Tab');
  await expect(roster.mapColumn('Email')).toBeFocused();
  expect((await roster.previewButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await roster.previewButton.click();
  await expect(roster.previewRow('Mobile Youth')).toContainText('Ready');
  await expect.poll(() => roster.horizontalOverflow()).toBeLessThanOrEqual(1);
});
