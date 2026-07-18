import { expect, test } from '@playwright/test';
import { AboutPage, LandingPage, ProgramsPage } from '../pages';

interface SeededPublicContent {
  trackName: string;
  moduleName: string;
  badgeName: string;
  cohortName: string;
  neighbourhood: string;
  mentorName: string;
  termStart: string;
  termEnd: string;
}

// Traces to: L2-PUB-002
test('landing, programs, and about render the authoritative public content payload', async ({
  page,
  request,
}) => {
  const seeded = await request.post('/api/_test/public-content');
  expect(seeded.ok(), `public-content seed failed: ${await seeded.text()}`).toBeTruthy();
  const content = (await seeded.json()) as SeededPublicContent;

  const landing = new LandingPage(page);
  await landing.goto();
  await expect(landing.journeyStep('Apply')).toBeVisible();
  await expect(page.getByText('1', { exact: true }).first()).toBeVisible();

  const programs = new ProgramsPage(page);
  await programs.goto();
  await expect(programs.track(content.trackName)).toBeVisible();
  await expect(programs.module(content.moduleName)).toBeVisible();
  await expect(programs.outcomeBadge(content.badgeName)).toBeVisible();
  await expect(page.getByText('Weekly check-ins', { exact: true })).toBeVisible();

  const about = new AboutPage(page);
  await about.goto();
  await expect(about.cohort(content.cohortName)).toBeVisible();
  await expect(page.getByText(content.neighbourhood, { exact: true })).toBeVisible();
  await expect(page.getByText(content.mentorName, { exact: true })).toBeVisible();
  await expect(page.getByText('Upcoming', { exact: true })).toBeVisible();
  await expect(page.getByText(/Sep 8, 2999.*Dec 15, 2999/i)).toBeVisible();
});
