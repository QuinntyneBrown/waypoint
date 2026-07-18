import { expect, test } from '@playwright/test';
import { PersonDetailPage, PeopleDirectoryPage, SignInPage } from '../../pages';
import { latestTestEmail, registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ADMIN-002, L2-CONSENT-003
test('an admin can inspect every section of a youth record and send a password reset', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'person-record');
  await new SignInPage(page).signIn(admin);

  const detail = new PersonDetailPage(page);
  await detail.goto(seeded.youthId);
  await expect(detail.heading).toHaveText(seeded.youthName);

  await expect(detail.panel('Profile')).toContainText(seeded.youthEmail);
  await expect(detail.panel('Profile')).toContainText(seeded.cohortName);
  await detail.tab('Guardian & consent').click();
  await expect(detail.panel('Guardian & consent')).toContainText(seeded.guardianName);
  await expect(detail.panel('Guardian & consent')).toContainText(seeded.guardianEmail);
  await expect(detail.panel('Guardian & consent')).toContainText('Valid');
  await detail.tab('Progress').click();
  await expect(detail.panel('Progress')).toContainText('1 of 2 lessons complete');
  await detail.tab('Attendance').click();
  await expect(detail.panel('Attendance')).toContainText(seeded.progressPresentSessionTitle);
  await detail.tab('Notes').click();
  await expect(detail.panel('Notes')).toContainText('1:1');
  await expect(page.getByText('PRIVATE PRAYER DETAIL', { exact: false })).toHaveCount(0);
  await detail.tab('Account').click();
  await expect(detail.panel('Account')).toContainText('Active');

  await detail.sendPasswordReset.click();
  await expect(detail.outcome).toContainText('Password reset sent');
  await expect
    .poll(async () => (await latestTestEmail(request, seeded.youthEmail)).subject)
    .toContain('Reset');
});

// Traces to: L2-ADMIN-002, L2-CONSENT-003
test('role warning, reversible deactivation, and irreversible removal are explicit', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'person-lifecycle');
  const person = await registerUser(request, 'Volunteer', 'lifecycle-target');
  await new SignInPage(page).signIn(admin);

  const detail = new PersonDetailPage(page);
  await detail.goto(person.userId);
  await detail.tab('Account').click();

  await detail.roleSelect.selectOption('Mentor');
  await detail.changeRole.click();
  const roleDialog = detail.dialog('Change account role?');
  await expect(roleDialog).toContainText("access to young people's records");
  await roleDialog.getByRole('button', { name: 'Confirm role change' }).click();
  await expect(detail.panel('Account')).toContainText('Mentor');

  await detail.deactivate.click();
  await detail
    .dialog('Deactivate account?')
    .getByRole('button', { name: 'Confirm deactivation' })
    .click();
  await expect(detail.panel('Account')).toContainText('Deactivated');

  await detail.reactivate.click();
  await detail
    .dialog('Reactivate account?')
    .getByRole('button', { name: 'Confirm reactivation' })
    .click();
  await expect(detail.panel('Account')).toContainText('Active');

  await detail.remove.click();
  const removeDialog = detail.dialog('Permanently remove person?');
  await expect(removeDialog).toContainText('cannot be undone');
  await removeDialog.getByLabel(`Type ${person.displayName} to confirm`).fill(person.displayName);
  await removeDialog.getByRole('button', { name: 'Permanently remove' }).click();
  await expect(detail.panel('Account')).toContainText('Removed');
  await expect(detail.reactivate).toHaveCount(0);

  const people = new PeopleDirectoryPage(page);
  await people.goto();
  await people.search(person.email);
  await expect(people.emptyState).toBeVisible();
});
