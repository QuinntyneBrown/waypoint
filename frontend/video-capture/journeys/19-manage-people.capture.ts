import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { PeopleDirectoryPage, PersonDetailPage, SignInPage } from '../../e2e/pages';
import { SeededUser } from '../../e2e/support/api';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/19-manage-people';

async function seedUser(
  request: APIRequestContext,
  role: string,
  displayName: string,
  tag = crypto.randomUUID().slice(0, 8),
): Promise<SeededUser> {
  const email = `${displayName.split(' ')[0].toLowerCase()}.${tag}@waypoint.demo`;
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

test('capture: manage people', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const tag = crypto.randomUUID().slice(0, 6);
  for (const [role, name] of [
    ['Youth', 'Amara Bell'],
    ['Youth', 'David Osei'],
    ['Youth', 'Imani Clarke'],
    ['Guardian', 'Ruth Clarke'],
    ['Mentor', 'Grace Adeyemi'],
  ]) {
    await seedUser(request, role, name, tag);
  }
  const sam = await seedUser(request, 'Volunteer', 'Sam Whitfield', tag);
  const seeded = await seedCast(request, 'Amara Bell');
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await expect(people.searchInput).toBeVisible();
  await people.search(tag);
  await expect(page.getByTestId('person-row')).toHaveCount(6);
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'directory', { x: 244, y: 90, width: 1036, height: 710 });

  await people.roleFilter('Youth').click();
  await expect(page.getByTestId('person-row')).toHaveCount(3);
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'filter', { x: 244, y: 90, width: 1036, height: 710 });

  const detail = new PersonDetailPage(page);
  await detail.goto(seeded.youthId);
  await expect(detail.heading).toHaveText(seeded.youthName);
  await expect(detail.panel('Profile')).toContainText(seeded.youthEmail);
  await detail.tab('Progress').click();
  await expect(detail.panel('Progress')).toContainText('1 of 2 lessons complete');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'record');

  await detail.goto(sam.userId);
  await detail.tab('Account').click();
  await expect(detail.sendPasswordReset).toBeVisible();
  await detail.roleSelect.selectOption('Mentor');
  await detail.changeRole.click();
  const roleDialog = detail.dialog('Change account role?');
  await expect(roleDialog).toContainText("access to young people's records");
  await page.waitForTimeout(600);
  await shotOf(page, 'role-dialog', roleDialog, 20);
  await roleDialog.getByRole('button', { name: 'Confirm role change' }).click();
  await expect(detail.panel('Account')).toContainText('Mentor');

  await detail.deactivate.click();
  const deactivate = detail.dialog('Deactivate account?');
  await expect(deactivate).toBeVisible();
  await page.waitForTimeout(600);
  await shotOf(page, 'deactivate-dialog', deactivate, 20);
  await deactivate.getByRole('button', { name: 'Confirm deactivation' }).click();
  await expect(detail.panel('Account')).toContainText('Deactivated');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'deactivated');
});
