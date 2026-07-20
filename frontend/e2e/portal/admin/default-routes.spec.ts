import { expect, test } from '@playwright/test';
import { registerUser } from '../../support/api';
import { AdminDashboardPage, InvitationsPage, SignInPage } from '../../pages';

// Traces to: L2-AUTH-001, L2-REPORT-001
test('a program administrator lands on the report-backed director dashboard', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'director-home');
  await new SignInPage(page).signIn(admin);

  await expect(page).toHaveURL(AdminDashboardPage.path);
  const dashboard = new AdminDashboardPage(page);
  await expect(dashboard.heading).toBeVisible();
  await expect(dashboard.summary).toBeVisible();
});

// Traces to: L2-AUTH-003, L2-AUTH-004
test('an administrator can send and track an invitation', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'invite-manager');
  await new SignInPage(page).signIn(admin);
  const invitations = new InvitationsPage(page);
  await invitations.goto();

  const email = `invite-${crypto.randomUUID()}@wordup.test`;
  await invitations.emails.fill(email);
  await invitations.sendButton.click();
  await expect(page.getByText('Invitation sent.', { exact: true })).toBeVisible();
  await expect(invitations.rows.filter({ hasText: email })).toBeVisible();
});
