import { APIRequestContext, Locator, Page, expect, test } from '@playwright/test';
import { PeopleDirectoryPage, SignInPage } from '../e2e/pages';
import { SeededUser } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '029-program-administration-search-people';

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

test('capture: search people', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const tag = crypto.randomUUID().slice(0, 6);
  const cast: [string, string, string][] = [
    ['Youth', 'Amara Bell', 'amara.bell'],
    ['Youth', 'David Osei', 'david.osei'],
    ['Youth', 'Imani Clarke', 'imani.clarke'],
    ['Guardian', 'Ruth Clarke', 'ruth.clarke'],
    ['Volunteer', 'Sam Whitfield', 'sam.whitfield'],
    ['Mentor', 'Grace Adeyemi', 'grace.adeyemi'],
  ];
  for (const [role, displayName, local] of cast) {
    const res = await request.post('/api/_test/users', {
      data: { email: `${local}.${tag}@waypoint.demo`, password: 'Password123', displayName, role },
    });
    expect(res.ok()).toBeTruthy();
  }
  const admin = await seedUser(request, 'ProgramAdministrator', 'Pat Morgan');
  await new SignInPage(page).signIn(admin);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await expect(people.searchInput).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'directory', { x: 244, y: 90, width: 1036, height: 340 });

  await people.search(tag);
  await expect(page.getByTestId('person-row')).toHaveCount(6);
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'search', { x: 244, y: 90, width: 1036, height: 710 });

  await people.roleFilter('Youth').click();
  await expect(page.getByTestId('person-row')).toHaveCount(3);
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'filters', { x: 244, y: 90, width: 1036, height: 710 });

  await people.roleFilter('All roles').click();
  await people.search(`clarke.${tag}`);
  await expect(page.getByTestId('person-row')).toHaveCount(2);
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'name', { x: 244, y: 90, width: 1036, height: 710 });

  await people.search(`nobody-${crypto.randomUUID()}`);
  await expect(people.emptyState).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'empty', { x: 244, y: 90, width: 1036, height: 710 });
});
