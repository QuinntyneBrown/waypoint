import { expect, test } from '@playwright/test';
import {
  latestTestEmail,
  registerUser,
  seedYouthCurriculum,
  submitApplication,
} from '../../support/api';
import { ApplicationsListPage, SignInPage } from '../../pages';

// Traces to: L2-INTAKE-002
test('an admin sees a submitted application in the review queue', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-apps');
  const application = await submitApplication(request, 'review');
  await new SignInPage(page).signIn(admin);

  const applications = new ApplicationsListPage(page);
  await applications.goto();
  await expect(page).toHaveURL(ApplicationsListPage.urlPattern);

  // The anonymously-submitted application appears in the admin review queue (AC1).
  await expect(applications.rowFor(application.lastName)).toBeVisible();
});

// Traces to: L2-INTAKE-002
test('admin reviews consent state, declines softly, and approves a valid placement', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-decisions');
  const minor = await submitApplication(request, 'minor-consent');
  const adultLastName = 'Adult-' + crypto.randomUUID().slice(0, 8);
  const adultEmail = 'adult-' + crypto.randomUUID() + '@example.com';
  const adultSubmission = await request.post('/api/applications', {
    data: {
      firstName: 'Jordan',
      lastName: adultLastName,
      age: 18,
      whoIsFilling: 'self',
      neighbourhood: 'Rexdale',
      cohortInterest: seeded.cohortName,
      whyInterested: 'I want to grow as a leader and serve.',
      trackInterest: seeded.primaryTrackName,
      guardianName: 'Jordan Contact',
      guardianEmail: adultEmail,
      guardianPhone: '4165550188',
      participationConsentAck: true,
      privacyAck: true,
    },
  });
  expect(
    adultSubmission.ok(),
    'adult application failed: ' + adultSubmission.status() + ' ' + (await adultSubmission.text()),
  ).toBeTruthy();

  await new SignInPage(page).signIn(admin);
  const applications = new ApplicationsListPage(page);
  await applications.goto();

  await applications.statusFilter.selectOption({ label: 'Needs consent' });
  await expect(applications.rowFor(minor.lastName)).toContainText('Needs consent');
  await applications.review(minor.lastName).click();
  await expect(applications.reviewDialog).toContainText('Guardian participation consent');
  await expect(applications.reviewDialog).toContainText('I want to grow as a leader and serve.');
  await expect(applications.approve).toBeDisabled();
  await applications.decisionNote.fill('No place is available this term.');
  await applications.decline.click();
  const declineDialog = page.getByRole('dialog', { name: /decline application/i });
  await expect(declineDialog).toContainText(minor.lastName);
  await declineDialog.getByRole('button', { name: /confirm decline/i }).click();
  await expect(applications.reviewDialog).toHaveCount(0);

  await applications.statusFilter.selectOption({ label: 'Declined' });
  await expect(applications.rowFor(minor.lastName)).toContainText('Declined');

  await applications.statusFilter.selectOption('');
  await applications.review(adultLastName).click();
  await applications.trackInterest.fill(seeded.primaryTrackName);
  await applications.cohort.selectOption(seeded.cohortId);
  await applications.mentor.selectOption(seeded.mentorId);
  await applications.decisionNote.fill('Excellent fit for this cohort.');
  await applications.approve.click();
  const approveDialog = page.getByRole('dialog', { name: /approve application/i });
  await expect(approveDialog).toContainText(seeded.cohortName);
  await expect(approveDialog).toContainText(seeded.mentorName);
  await approveDialog.getByRole('button', { name: /confirm approval/i }).click();
  await expect(applications.reviewDialog).toHaveCount(0);

  await applications.statusFilter.selectOption({ label: 'Approved' });
  await expect(applications.rowFor(adultLastName)).toContainText('Approved');
  const email = await latestTestEmail(request, adultEmail);
  expect(email.subject).toMatch(/approved/i);
});
