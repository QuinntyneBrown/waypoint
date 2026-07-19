import { expect, test } from '@playwright/test';
import { AdminAuditLogPage, SignInPage } from '../pages';
import { registerUser } from '../support/api';

// Traces to: L2-CONSENT-003, L2-NFR-009, L2-NFR-010
test('audit filters, cards, paging, and export remain touch usable on mobile', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'audit-mobile');
  await new SignInPage(page).signIn(admin);
  const audit = new AdminAuditLogPage(page);
  await audit.goto();

  await expect(audit.heading).toBeVisible();
  expect((await audit.exportCsv.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect((await audit.applyFilters.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect(audit.rows.first()).toBeVisible();
  await expect.poll(() => audit.horizontalOverflow()).toBeLessThanOrEqual(1);
});
