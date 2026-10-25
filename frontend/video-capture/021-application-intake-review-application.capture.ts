import { Page, expect, test } from '@playwright/test';
import { ApplicationsListPage, SignInPage } from '../e2e/pages';
import { registerUser } from '../e2e/support/api';
import { join } from 'node:path';
import { shot } from './shots';

const FOLDER = '021-application-intake-review-application';
const QUEUE = { x: 0, y: 60, width: 1280, height: 740 };

async function tall(page: Page, name: string): Promise<void> {
  await page.setViewportSize({ width: 1280, height: 1240 });
  await page.waitForTimeout(800);
  await page.screenshot({
    path: join(__dirname, '..', '..', 'docs', 'videos', FOLDER, 'images', `${name}-1280.png`),
    clip: { x: 240, y: 0, width: 800, height: 1240 },
  });
  await page.setViewportSize({ width: 1280, height: 800 });
}

test('capture: review an application', async ({ page, request }) => {
  const admin = await registerUser(request, 'ProgramAdministrator', 'admin-review');
  const application = { lastName: `Clarke-${crypto.randomUUID().slice(0, 4)}` };
  const submitted = await request.post('/api/applications', {
    data: {
      firstName: 'Imani',
      lastName: application.lastName,
      age: 15,
      whoIsFilling: 'guardian',
      neighbourhood: 'Peckham',
      cohortInterest: 'Peckham Autumn cohort',
      whyInterested: 'I want to grow as a leader and serve.',
      trackInterest: 'Bible & Leadership',
      guardianName: 'Ruth Clarke',
      guardianEmail: 'ruth.clarke@example.com',
      guardianPhone: '4165550100',
      participationConsentAck: true,
      privacyAck: true,
    },
  });
  expect(submitted.ok()).toBeTruthy();

  await new SignInPage(page).signIn(admin);
  const applications = new ApplicationsListPage(page);
  await applications.goto();
  await expect(applications.rowFor(application.lastName)).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'queue', QUEUE);

  await applications.statusFilter.selectOption({ label: 'Needs consent' });
  await expect(applications.rowFor(application.lastName)).toContainText('Needs consent');
  await shot(page, FOLDER, 'filter', QUEUE);

  await applications.review(application.lastName).click();
  await expect(applications.reviewDialog).toContainText('I want to grow as a leader and serve.');
  await expect(applications.approve).toBeEnabled();
  await tall(page, 'detail');
});
