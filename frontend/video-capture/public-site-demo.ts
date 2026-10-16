import { Locator, Page } from '@playwright/test';

/** Public content with demo names, rewritten on the way to the browser; the pages are the real ones. */
function demoContent(json: any) {
  const now = Date.now();
  const day = 86_400_000;
  const iso = (ms: number) => new Date(ms).toISOString().replace(/\.\d+Z$/, 'Z');
  const track = (id: string, name: string, description: string, modules: [string, string][]) => ({
    id,
    name,
    key: id,
    description,
    modules: modules.map(([moduleName, badge], i) => ({
      id: `${id}-m${i}`,
      name: moduleName,
      description: moduleName,
      order: i + 1,
      outcomeBadge: { id: `${id}-b${i}`, name: badge, icon: 'heart', description: badge },
    })),
  });
  json.tracks = [
    track('bible', 'Bible & Leadership', 'Study scripture and practise servant leadership.', [
      ['Servant Leadership', 'Servant Heart'],
      ['Prayer and Purpose', 'Steady Voice'],
    ]),
    track(
      'coding',
      'Coding for Kingdom',
      'Learn to build useful software for your neighbourhood.',
      [
        ['Web Foundations', 'First Build'],
        ['Build for Others', 'Team Builder'],
      ],
    ),
  ];
  json.cohorts = [
    {
      id: 'c1',
      name: 'Regent Park Fall Cohort',
      neighbourhood: 'Regent Park',
      termStart: iso(now + 30 * day),
      termEnd: iso(now + 120 * day),
      leadMentor: 'Grace Adeyemi',
      status: 'Upcoming',
    },
    {
      id: 'c2',
      name: 'Rexdale Summer Cohort',
      neighbourhood: 'Rexdale',
      termStart: iso(now - 20 * day),
      termEnd: iso(now + 60 * day),
      leadMentor: 'Grace Adeyemi',
      status: 'Active',
    },
  ];
  json.impact = { tracks: 2, cohorts: 2, modules: 4, neighbourhoods: 2 };
  return json;
}

/**
 * The public site renders its first page on the server, so a browser route cannot change it.
 * Load a neutral page first, then open the target through the in-app link so the browser
 * fetches the content itself and receives the demo names.
 */
export async function openWithDemoContent(
  page: Page,
  link: string,
  transform: (json: any) => any = (j) => j,
): Promise<void> {
  await page.route('**/api/public/content', async (route) => {
    const res = await route.fetch();
    await route.fulfill({ response: res, json: transform(demoContent(await res.json())) });
  });
  await page.goto('/privacy', { waitUntil: 'networkidle' });
  if (link === 'home') {
    await page.getByRole('link', { name: 'Waypoint' }).first().click();
  } else {
    await page
      .getByRole('navigation', { name: 'Primary' })
      .getByRole('link', { name: link, exact: true })
      .click();
  }
}

/** Scroll an element into view and return its box for use as a screenshot clip. */
export async function clipOf(
  target: Locator,
  pad = 0,
  top?: number,
): Promise<{ x: number; y: number; width: number; height: number }> {
  await target.scrollIntoViewIfNeeded();
  if (top !== undefined) {
    // Park the element below the sticky header so the header never overlaps the crop.
    await target.evaluate((el, y) => window.scrollBy(0, el.getBoundingClientRect().top - y), top);
  }
  const box = (await target.boundingBox())!;
  return {
    x: Math.max(0, box.x - pad),
    y: Math.max(0, box.y - pad),
    width: Math.min(1280, box.width + 2 * pad),
    height: box.height + 2 * pad,
  };
}
