import { Page, expect, test } from '@playwright/test';
import { ApplicationsListPage, SignInPage } from '../e2e/pages';
import { registerUser, seedYouthCurriculum } from '../e2e/support/api';
import { join } from 'node:path';
import { shot } from './shots';

const FOLDER = '020-application-intake-decide-and-enroll-applicant';
const CONFIRM = { x: 340, y: 200, width: 600, height: 400 };

async function tall(page: Page, name: string): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 1240 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: join(__dirname, '..', '..', 'docs', 'videos', FOLDER, 'images', `${name}-1280.png`),
    clip: { x: 240, y: 0, width: 800, height: 1240 },
  });
  await page.setViewportSize({ width: 1280, height: 800 });
}

test('capture: decide and enroll an applicant', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-decide');
  const minorLastName = `Clarke-${crypto.randomUUID().slice(0, 4)}`;
  const minorSubmission = await request.post('/api/applications', {
    data: {
      firstName: 'Imani',
      lastName: minorLastName,
      age: 15,
      whoIsFilling: 'guardian',
      neighbourhood: 'Peckham',
      cohortInterest: seeded.cohortName,
      whyInterested: 'I want to grow as a leader and serve.',
      trackInterest: seeded.primaryTrackName,
      guardianName: 'Ruth Clarke',
      guardianEmail: 'ruth.clarke@example.com',
      guardianPhone: '4165550100',
      participationConsentAck: true,
      privacyAck: true,
    },
  });
  expect(minorSubmission.ok()).toBeTruthy();
  const adultLastName = `Bell-${crypto.randomUUID().slice(0, 4)}`;
  const adultEmail = `adult-${crypto.randomUUID()}@example.com`;
  const adult = await request.post('/api/applications', {
    data: {
      firstName: 'Amara',
      lastName: adultLastName,
      age: 18,
      whoIsFilling: 'self',
      neighbourhood: 'Peckham',
      cohortInterest: seeded.cohortName,
      whyInterested: 'I want to grow as a leader and serve.',
      trackInterest: seeded.primaryTrackName,
      guardianName: 'Amara Bell',
      guardianEmail: adultEmail,
      guardianPhone: '4165550188',
      participationConsentAck: true,
      privacyAck: true,
    },
  });
  expect(adult.ok()).toBeTruthy();

  await new SignInPage(page).signIn(admin);
  const applications = new ApplicationsListPage(page);
  await applications.goto();
  await expect(applications.rowFor(adultLastName)).toBeVisible();

  // A minor without consent can be approved but cannot take part until consent is linked.
  await applications.review(minorLastName).click();
  await expect(applications.reviewDialog).toContainText('Guardian participation consent');
  await expect(applications.approve).toBeEnabled();
  await tall(page, 'blocked');

  // Soft decline with a note.
  await applications.decisionNote.fill('No place is available this term.');
  await applications.decline.click();
  const declineDialog = page.getByRole('dialog', { name: /decline application/i });
  await expect(declineDialog).toBeVisible();
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'decline-confirm', CONFIRM);
  await declineDialog.getByRole('button', { name: /confirm decline/i }).click();
  await expect(applications.reviewDialog).toHaveCount(0);
  await expect(page.getByRole('status').filter({ hasText: /declined/ })).toBeVisible();
  await applications.statusFilter.selectOption({ label: 'Declined' });
  await expect(applications.rowFor(minorLastName)).toContainText('Declined');
  await expect(applications.rowFor(adultLastName)).toHaveCount(0);
  await shot(page, FOLDER, 'declined');

  // Approve a placement.
  await applications.statusFilter.selectOption('');
  await applications.review(adultLastName).click();
  await applications.trackInterest.fill(seeded.primaryTrackName);
  await applications.cohort.selectOption(seeded.cohortId);
  await applications.mentor.selectOption(seeded.mentorId);
  await applications.decisionNote.fill('Excellent fit for this cohort.');
  await expect(applications.approve).toBeEnabled();
  await tall(page, 'placement');
  await applications.approve.click();
  const approveDialog = page.getByRole('dialog', { name: /approve application/i });
  await expect(approveDialog).toContainText(seeded.cohortName);
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'approve-confirm', CONFIRM);
  await approveDialog.getByRole('button', { name: /confirm approval/i }).click();
  await expect(applications.reviewDialog).toHaveCount(0);
  await expect(page.getByRole('status').filter({ hasText: /approved and enrolled/ })).toBeVisible();
  await applications.statusFilter.selectOption({ label: 'Approved' });
  await expect(applications.rowFor(adultLastName)).toContainText('Approved');
  await expect(applications.rowFor(minorLastName)).toHaveCount(0);
  await shot(page, FOLDER, 'approved');
});
