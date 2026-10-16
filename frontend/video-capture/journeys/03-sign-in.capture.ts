import { expect, test } from '@playwright/test';
import { MentorDashboardPage, SignInPage, YouthHomePage } from '../../e2e/pages';
import { shot } from '../shots';

const CARD = { x: 380, y: 90, width: 520, height: 620 };
const LOCKED = { x: 340, y: 240, width: 600, height: 320 };

const FOLDER = 'journeys/03-sign-in';
const password = 'Password123';

async function seed(request: any, displayName: string, role: string, prefix: string) {
  const email = `${prefix}-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok()).toBeTruthy();
  return email;
}

test('capture: journey sign in', async ({ page, request, browser }) => {
  const amara = await seed(request, 'Amara Bell', 'Youth', 'amara');
  const grace = await seed(request, 'Grace Adeyemi', 'Mentor', 'grace');

  const signIn = new SignInPage(page);
  await signIn.goto();
  await expect(signIn.submitButton).toBeVisible();
  await shot(page, FOLDER, 'form', CARD);

  await signIn.email.fill(amara);
  await signIn.password.fill('WrongPassword9');
  await signIn.submitButton.click();
  await expect(signIn.errorAlert).toBeVisible();
  await shot(page, FOLDER, 'error', { ...CARD, y: 40, height: 680 });

  await signIn.password.fill(password);
  await signIn.submitButton.click();
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'youth-home');

  const mentorPage = await browser.newPage({ baseURL: 'http://localhost:4200' });
  const mentorSignIn = new SignInPage(mentorPage);
  await mentorSignIn.signIn({ email: grace, password });
  await expect(mentorPage).toHaveURL(/\/mentor\//);
  await mentorPage.waitForLoadState('networkidle');
  await shot(mentorPage, FOLDER, 'mentor-home');

  const imani = await seed(request, 'Imani Clarke', 'Youth', 'imani');
  const fresh = await browser.newPage({ baseURL: 'http://localhost:4200' });
  const lockedSignIn = new SignInPage(fresh);
  await lockedSignIn.goto();
  await lockedSignIn.email.fill(imani);
  for (let i = 0; i < 5; i++) {
    await lockedSignIn.password.fill('WrongPassword9');
    await lockedSignIn.submitButton.click();
    if (i < 4) await expect(lockedSignIn.errorAlert).toBeVisible();
  }
  await expect(fresh).toHaveURL(/\/auth\/account-locked/);
  await fresh.waitForLoadState('networkidle');
  await shot(fresh, FOLDER, 'locked', LOCKED);
  void MentorDashboardPage;
});
