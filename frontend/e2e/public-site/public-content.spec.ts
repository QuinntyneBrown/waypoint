import { expect, test } from '@playwright/test';
import { AboutPage, LandingPage, ProgramsPage } from '../pages';

const content = {
  tracks: [
    {
      id: '10000000-0000-0000-0000-000000000001',
      name: 'API Leadership Track',
      key: 'api-leadership',
      description: 'Leadership content served by the authoritative API.',
      modules: [
        {
          id: '20000000-0000-0000-0000-000000000001',
          name: 'API Servant Leadership Module',
          description: 'Lead by serving.',
          order: 1,
          outcomeBadge: {
            id: '30000000-0000-0000-0000-000000000001',
            name: 'API Servant Heart Badge',
            icon: 'heart',
            description: 'Complete the module.',
          },
        },
      ],
    },
  ],
  cohorts: [
    {
      id: '40000000-0000-0000-0000-000000000001',
      name: 'API Regent Park Cohort',
      neighbourhood: 'Regent Park',
      termStart: '2026-09-08T00:00:00Z',
      termEnd: '2026-12-15T00:00:00Z',
      leadMentor: 'Pat API Mentor',
      status: 'Upcoming',
    },
  ],
  mentorship: {
    summary: 'API mentorship summary.',
    features: ['API weekly check-ins', 'API reflection reviews'],
  },
  journey: [
    { step: 1, title: 'API Apply', description: 'Apply from the API.' },
    { step: 2, title: 'API Join', description: 'Join from the API.' },
  ],
  impact: { tracks: 1, cohorts: 1, modules: 1, neighbourhoods: 1 },
};

// Traces to: L2-PUB-002
test('landing, programs, and about render the authoritative public content payload', async ({
  page,
}) => {
  let requests = 0;
  await page.route('**/api/public/content', async (route) => {
    requests += 1;
    await route.fulfill({ status: 200, contentType: 'application/json', json: content });
  });

  const landing = new LandingPage(page);
  await landing.goto();
  await expect(landing.journeyStep('API Apply')).toBeVisible();
  await expect(page.getByText('1', { exact: true }).first()).toBeVisible();

  const programs = new ProgramsPage(page);
  await programs.goto();
  await expect(programs.track('API Leadership Track')).toBeVisible();
  await expect(programs.module('API Servant Leadership Module')).toBeVisible();
  await expect(programs.outcomeBadge('API Servant Heart Badge')).toBeVisible();
  await expect(page.getByText('API weekly check-ins', { exact: true })).toBeVisible();

  const about = new AboutPage(page);
  await about.goto();
  await expect(about.cohort('API Regent Park Cohort')).toBeVisible();
  await expect(page.getByText('Regent Park', { exact: true })).toBeVisible();
  await expect(page.getByText('Pat API Mentor', { exact: true })).toBeVisible();
  await expect(page.getByText('Upcoming', { exact: true })).toBeVisible();
  await expect(page.getByText(/Sep 8, 2026.*Dec 15, 2026/i)).toBeVisible();

  expect(requests).toBeGreaterThanOrEqual(3);
});
