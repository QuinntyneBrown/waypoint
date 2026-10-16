import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { PersonDetailPage, SignInPage } from '../e2e/pages';
import { SeededUser, latestTestEmail } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '026-program-administration-manage-account-lifecycle';

async function seedUser(
  request: APIRequestContext,
  role: string,
  displayName: string,
): Promise<SeededUser> {
  const email = `${displayName.split(' ')[0].toLowerCase()}-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok()).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, displayName, role, userId };
}

/** Crop a screenshot to an element, with a little padding. */
async function shotOf(page: Page, name: string, target: Locator, pad = 12): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  const box = (await target.boundingBox())!;
  await shot(page, FOLDER, name, {
    x: Math.max(0, box.x - pad),
    y: Math.max(0, box.y - pad),
    width: Math.min(1280, box.width + pad * 2),
    height: Math.min(800, box.height + pad * 2),
  });
}

test('capture: manage account lifecycle', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  const sam = await seedUser(request, 'Volunteer', 'Sam Whitfield');
  await new SignInPage(page).signIn(admin);

  const detail = new PersonDetailPage(page);
  await detail.goto(sam.userId);
  await detail.tab('Account').click();
  await expect(detail.sendPasswordReset).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'account');

  await detail.sendPasswordReset.click();
  await expect(detail.outcome).toContainText('Password reset sent');
  await expect
    .poll(async () => (await latestTestEmail(request, sam.email)).subject)
    .toContain('Reset');
  await shot(page, FOLDER, 'reset');

  await detail.roleSelect.selectOption('Mentor');
  await detail.changeRole.click();
  const roleDialog = detail.dialog('Change account role?');
  await expect(roleDialog).toContainText("access to young people's records");
  await shotOf(page, 'role-dialog', roleDialog, 20);
  await roleDialog.getByRole('button', { name: 'Confirm role change' }).click();
  await expect(detail.panel('Account')).toContainText('Mentor');

  await detail.deactivate.click();
  const deactivate = detail.dialog('Deactivate account?');
  await expect(deactivate).toBeVisible();
  await shotOf(page, 'deactivate-dialog', deactivate, 20);
  await deactivate.getByRole('button', { name: 'Confirm deactivation' }).click();
  await expect(detail.panel('Account')).toContainText('Deactivated');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'deactivated');

  await detail.reactivate.click();
  await detail
    .dialog('Reactivate account?')
    .getByRole('button', { name: 'Confirm reactivation' })
    .click();
  await expect(detail.panel('Account')).toContainText('Active');

  await detail.remove.click();
  const removeDialog = detail.dialog('Permanently remove person?');
  await expect(removeDialog).toContainText('cannot be undone');
  await removeDialog.getByLabel(`Type ${sam.displayName} to confirm`).fill(sam.displayName);
  await shotOf(page, 'remove-dialog', removeDialog, 20);
});
