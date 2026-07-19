import { expect, test } from '@playwright/test';
import { AdminRosterImportPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-REPORT-004, L2-NFR-001, L2-CONSENT-003
test('an admin maps, previews, confirms, and imports only ready roster rows', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'roster-import');
  const readyEmail = `ready.${crypto.randomUUID()}@example.com`;
  const consentEmail = `consent.${crypto.randomUUID()}@example.com`;
  const csv = [
    'Full Name,Email Address,Group,Leader,Stream,Notes,Unused',
    `Alice Ready,${readyEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `Existing Youth,${seeded.youthEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
    `Consent Follow-up,${consentEmail},${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},MissingConsent,ignore me`,
    `Broken Email,not-an-email,${seeded.cohortName},${seeded.mentorName},${seeded.primaryTrackName},Active,ignore me`,
  ].join('\n');

  await new SignInPage(page).signIn(admin);
  const roster = new AdminRosterImportPage(page);
  await roster.goto();
  await expect(roster.heading).toBeVisible();
  await roster.fileInput.setInputFiles({
    name: 'summer-roster.csv',
    mimeType: 'text/csv',
    buffer: Buffer.from(csv),
  });

  await expect(roster.fileSummary).toContainText('4 rows · 7 columns');
  await expect(roster.mapColumn('Full Name')).toHaveValue('Name');
  await expect(roster.mapColumn('Email Address')).toHaveValue('Email');
  await expect(roster.mapColumn('Group')).toHaveValue('Cohort');
  await expect(roster.mapColumn('Leader')).toHaveValue('Mentor');
  await expect(roster.mapColumn('Stream')).toHaveValue('Track');
  await roster.mapColumn('Notes').selectOption('Status');
  await expect(roster.mapColumn('Unused')).toHaveValue('Skip');

  await roster.previewButton.click();
  await expect(roster.previewSummary).toContainText('1 ready');
  await expect(roster.previewSummary).toContainText('1 needs consent');
  await expect(roster.previewSummary).toContainText('1 duplicate');
  await expect(roster.previewSummary).toContainText('1 invalid');
  await expect(roster.previewRow(readyEmail)).toContainText('Ready');
  await expect(roster.previewRow(seeded.youthEmail)).toContainText('Duplicate');
  await expect(roster.previewRow(consentEmail)).toContainText('Missing consent');
  await expect(roster.previewRow('not-an-email')).toContainText('Invalid');

  await roster.importReady.click();
  await expect(roster.confirmation).toContainText('Only 1 ready row will create an account');
  await roster.confirmImport.click();
  await expect(roster.result).toContainText('Import complete');
  await expect(roster.result).toContainText('1 imported');
  await expect(roster.result).toContainText('1 duplicate skipped');
  await expect(roster.result).toContainText('1 needs consent');
  await expect(roster.result).toContainText('1 invalid');
});

// Traces to: L2-REPORT-004 AC1..2, L2-NFR-001
test('the import page rejects wrong file types, oversized CSVs, and incomplete mapping', async ({
  page,
  request,
}) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'roster-validation');
  await new SignInPage(page).signIn(admin);
  const roster = new AdminRosterImportPage(page);
  await roster.goto();

  await roster.fileInput.setInputFiles({
    name: 'roster.txt',
    mimeType: 'text/plain',
    buffer: Buffer.from('name,email\nAlice,alice@example.com'),
  });
  await expect(roster.fileError).toContainText('Choose a .csv file');

  await roster.fileInput.setInputFiles({
    name: 'too-large.csv',
    mimeType: 'text/csv',
    buffer: Buffer.alloc(2 * 1024 * 1024 + 1, 'a'),
  });
  await expect(roster.fileError).toContainText('2 MB or smaller');

  await roster.fileInput.setInputFiles({
    name: 'unknown.csv',
    mimeType: 'text/csv',
    buffer: Buffer.from('Person,Contact\nAlice,alice@example.com'),
  });
  await expect(roster.fileSummary).toContainText('1 row · 2 columns');
  await expect(roster.previewButton).toBeDisabled();
  await expect(page.getByText('Map exactly one column to Email')).toBeVisible();
});
