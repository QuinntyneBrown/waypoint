import { expect, test } from '@playwright/test';
import { AdminAnnouncementsPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-ENGAGE-006, L2-AUTH-010, L2-NFR-001
test('an admin sends and schedules targeted announcements and tracks recipient reads', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'announcements-admin');
  const signIn = new SignInPage(page);
  await signIn.signIn(admin);
  const announcements = new AdminAnnouncementsPage(page);
  await announcements.goto();

  await expect(announcements.heading).toBeVisible();

  const scheduledTitle = `Scheduled guardian update ${crypto.randomUUID().slice(0, 8)}`;
  await announcements.title.fill(scheduledTitle);
  await announcements.message.fill('Guardians will receive the final trip details tomorrow.');
  await announcements.audience.selectOption('org');
  await announcements.recipients.selectOption('Guardians');
  await announcements.scheduleLater.check();
  const tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000);
  await announcements.scheduledAt.fill(tomorrow.toISOString().slice(0, 16));
  await announcements.submit.click();
  await expect(announcements.status).toContainText('scheduled');
  await expect(announcements.announcement(scheduledTitle)).toContainText('Scheduled');
  await expect(announcements.announcement(scheduledTitle)).toContainText('Organization');
  await expect(announcements.announcement(scheduledTitle)).toContainText('Guardians');

  const sentTitle = `Cohort youth update ${crypto.randomUUID().slice(0, 8)}`;
  await announcements.title.fill(sentTitle);
  await announcements.message.fill('Youth night starts at 6 p.m. in the main studio.');
  await announcements.audience.selectOption('cohort');
  await announcements.cohort.selectOption(seeded.cohortId);
  await announcements.recipients.selectOption('Youth');
  await announcements.sendNow.check();
  await announcements.submit.click();
  await expect(announcements.status).toContainText('sent');
  const sent = announcements.announcement(sentTitle);
  await expect(sent).toContainText('Sent');
  await expect(sent).toContainText(seeded.cohortName);
  await expect(sent).toContainText('0 of 1 read');

  await page.getByRole('button', { name: 'Sign out', exact: true }).click();
  await signIn.signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  await page.goto('/youth/announcements');
  const recipientCard = page.locator('.wu-card').filter({ hasText: sentTitle });
  await expect(recipientCard).toBeVisible();
  await expect(recipientCard).toContainText('New');
  await page.getByRole('button', { name: 'Mark all read', exact: true }).click();
  await expect(recipientCard).not.toContainText('New');

  await page.getByRole('button', { name: 'Sign out', exact: true }).click();
  await signIn.signIn(admin);
  await announcements.goto();
  await expect(announcements.announcement(sentTitle)).toContainText('1 of 1 read');
  await expect(announcements.announcement(sentTitle)).toContainText('100% read');
});
