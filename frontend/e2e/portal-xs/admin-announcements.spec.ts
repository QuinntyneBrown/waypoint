import { expect, test } from '@playwright/test';
import { AdminAnnouncementsPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ENGAGE-006, L2-NFR-009, L2-NFR-010
test('announcement composition and history remain touch usable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'announcements-mobile');
  await new SignInPage(page).signIn(admin);
  const announcements = new AdminAnnouncementsPage(page);
  await announcements.goto();

  await expect(announcements.heading).toBeVisible();
  const title = `Mobile cohort update ${crypto.randomUUID().slice(0, 8)}`;
  await announcements.title.fill(title);
  await announcements.message.fill('A concise program update for the cohort.');
  await announcements.audience.selectOption('cohort');
  await announcements.cohort.selectOption(seeded.cohortId);
  await announcements.recipients.selectOption('Youth');
  expect((await announcements.submit.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await announcements.submit.click();
  await expect(announcements.status).toContainText('sent');
  await expect(announcements.announcement(title)).toBeVisible();
  await expect.poll(() => announcements.horizontalOverflow()).toBeLessThanOrEqual(1);
});
