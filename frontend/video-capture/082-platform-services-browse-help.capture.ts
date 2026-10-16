import { expect, Page, test } from '@playwright/test';
import { HelpPage, SignInPage } from '../e2e/pages';
import { shot } from './shots';

/** Sign in through the real form, allowing for slow password hashing on a busy shared API. */
async function signInSlow(page: Page, email: string, password: string) {
  const signIn = new SignInPage(page);
  await signIn.goto();
  await signIn.submitCredentials(email, password);
  await expect(page).not.toHaveURL(/\/auth\/sign-in/, { timeout: 30000 });
}

const FOLDER = '082-platform-services-browse-help';
const CONTENT = { x: 244, y: 80, width: 1036, height: 720 };

test('capture: browse help', async ({ page, request, browser }) => {
  test.setTimeout(240000);
  const email = `amara-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const displayName = 'Amara Bell';
  const registered = await request.post('/api/auth/register', {
    data: { email, password, displayName },
  });
  expect(registered.ok()).toBeTruthy();
  const auth = (await registered.json()) as { accessToken: string };
  const onboarded = await request.post('/api/onboarding', {
    headers: { Authorization: `Bearer ${auth.accessToken}` },
    data: { displayName },
  });
  expect(onboarded.ok()).toBeTruthy();

  await signInSlow(page, email, password);
  const help = new HelpPage(page);
  await help.goto();
  await expect(help.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'help', CONTENT);

  await page.getByText('Why is a lesson or quiz locked?').click();
  await expect(page.getByText(/Some activities have prerequisites/)).toBeVisible();
  await shot(page, FOLDER, 'answer', CONTENT);

  await help.search.fill('badges');
  await expect(page.getByText('Why is a lesson or quiz locked?')).toHaveCount(0);
  await shot(page, FOLDER, 'search', CONTENT);

  await help.search.fill('zzzzqq');
  await expect(page.getByRole('heading', { name: 'No help articles found' })).toBeVisible();
  await shot(page, FOLDER, 'none', CONTENT);

  const mentorEmail = `grace-${crypto.randomUUID()}@waypoint.demo`;
  const seeded = await request.post('/api/_test/users', {
    data: { email: mentorEmail, password, displayName: 'Grace Adeyemi', role: 'Mentor' },
  });
  expect(seeded.ok()).toBeTruthy();
  const mentorPage = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await signInSlow(mentorPage, mentorEmail, password);
  const mentorHelp = new HelpPage(mentorPage);
  await mentorHelp.goto();
  await expect(mentorHelp.heading).toBeVisible();
  await expect(mentorPage.getByText('How do I review a youth submission?')).toBeVisible();
  await mentorPage.waitForLoadState('networkidle');
  await shot(mentorPage, FOLDER, 'mentor', CONTENT);
});
