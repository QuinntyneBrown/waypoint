import { expect, test } from '@playwright/test';
import { PersonDetailPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-002, L2-NFR-009, L2-NFR-010
test('person record tabs and lifecycle controls remain usable without mobile overflow', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'person-mobile');
  await new SignInPage(page).signIn(admin);

  const detail = new PersonDetailPage(page);
  await detail.goto(seeded.youthId);
  const accountTab = detail.tab('Account');
  expect((await accountTab.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await accountTab.focus();
  await page.keyboard.press('Enter');
  await expect(detail.panel('Account')).toContainText('Send password reset');
  await expect.poll(() => detail.horizontalOverflow()).toBeLessThanOrEqual(1);
});
