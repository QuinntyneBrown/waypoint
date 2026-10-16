import { Page, expect, test } from '@playwright/test';
import { join } from 'node:path';
import { ApplicationsListPage, SignInPage } from '../../e2e/pages';
import { registerUser, seedYouthCurriculum } from '../../e2e/support/api';
import { shot } from '../shots';

const FOLDER = 'journeys/09-review-and-place';
const CONFIRM = { x: 340, y: 200, width: 600, height: 400 };
const QUEUE = { x: 0, y: 60, width: 1280, height: 740 };

async function tall(page: Page, name: string): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 1100 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: join(__dirname, '..', '..', '..', 'docs', 'videos', FOLDER, 'images', `${name}-1280.png`),
    clip: { x: 240, y: 0, width: 800, height: 1100 },
  });
  await page.setViewportSize({ width: 1280, height: 800 });
}

async function apply(
  request: import('@playwright/test').APIRequestContext,
  data: Record<string, unknown>,
): Promise<void> {
  const res = await request.post('/api/applications', {
    data: {
      neighbourhood: 'Peckham',
      whyInterested: 'I want to grow as a leader and serve.',
      guardianPhone: '4165550100',
      participationConsentAck: true,
      privacyAck: true,
      ...data,
    },
  });
  expect(res.ok()).toBeTruthy();
}

test('capture: review and place', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-place');
  const minorLastName = `Clarke-${crypto.randomUUID().slice(0, 4)}`;
  const adultLastName = `Bell-${crypto.randomUUID().slice(0, 4)}`;
  await apply(request, {
    firstName: 'Imani',
    lastName: minorLastName,
    age: 15,
    whoIsFilling: 'guardian',
    cohortInterest: seeded.cohortName,
    trackInterest: seeded.primaryTrackName,
    guardianName: 'Ruth Clarke',
    guardianEmail: 'ruth.clarke@example.com',
  });
  await apply(request, {
    firstName: 'Amara',
    lastName: adultLastName,
    age: 18,
    whoIsFilling: 'self',
    cohortInterest: seeded.cohortName,
    trackInterest: seeded.primaryTrackName,
    guardianName: 'Amara Bell',
    guardianEmail: `adult-${crypto.randomUUID()}@example.com`,
    guardianPhone: '4165550188',
  });

  await new SignInPage(page).signIn(admin);
  const applications = new ApplicationsListPage(page);
  await applications.goto();
  await expect(applications.rowFor(adultLastName)).toBeVisible();
  await page.waitForLoadState('networkidle');
  // The shared test database holds other applicants; show only this journey's two.
  await page.evaluate(
    (mine) =>
      document
        .querySelectorAll('[data-testid="application-row"]')
        .forEach((row) => mine.some((n) => row.textContent?.includes(n)) || row.remove()),
    [adultLastName, minorLastName],
  );
  await shot(page, FOLDER, 'queue', QUEUE);

  await applications.review(minorLastName).click();
  await expect(applications.reviewDialog).toContainText('Guardian participation consent');
  await expect(applications.approve).toBeDisabled();
  await tall(page, 'blocked');

  await applications.decisionNote.fill('No place is available this term.');
  await applications.decline.click();
  const declineDialog = page.getByRole('dialog', { name: /decline application/i });
  await expect(declineDialog).toBeVisible();
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'decline-confirm', CONFIRM);
  await declineDialog.getByRole('button', { name: /confirm decline/i }).click();
  await expect(applications.reviewDialog).toHaveCount(0);

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
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'approved');
});
