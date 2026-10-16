import { expect, test } from '@playwright/test';
import { SessionExpiredPage, SignInPage, YouthHomePage, YouthReflectionPage } from '../e2e/pages';
import { registerYouth, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '004-authentication-end-user-session';
const CARD = { x: 380, y: 90, width: 520, height: 620 };
const STATE = { x: 340, y: 230, width: 600, height: 340 };

test('capture: sign out', async ({ page, request }) => {
  const youth = await registerYouth(request, 'amara');
  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn(youth);
  await expect(page).toHaveURL(YouthHomePage.urlPattern);
  await page.waitForLoadState('networkidle');
  const home = new YouthHomePage(page);
  await expect(home.shell.signOutButton).toBeVisible();
  await shot(page, FOLDER, 'sign-out-button', { x: 0, y: 56, width: 244, height: 744 });

  await home.shell.signOut();
  await expect(page).toHaveURL(/\/auth\/sign-in/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'after-sign-out', CARD);

  // Going back to a protected page does not restore the session.
  await home.goto();
  await expect(page).toHaveURL(/\/auth\/sign-in/);
});

test('capture: session expiry keeps the draft', async ({ page, request }) => {
  await page.clock.install();
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const reflectionUrl = `/youth/lessons/${seeded.secondLessonId}/reflection`;
  await page.goto(reflectionUrl);

  const reflection = new YouthReflectionPage(page);
  await reflection.reflectionText.fill('Today I learned how to listen before I speak.');
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'draft');

  await page.clock.fastForward(20 * 60 * 1000);
  await page.keyboard.press('Shift');
  await page.clock.fastForward(20 * 60 * 1000);
  const signOutResponse = page.waitForResponse(
    (response) => response.url().includes('/api/auth/sign-out') && response.status() === 204,
    { timeout: 5_000 },
  );
  await page.clock.fastForward(11 * 60 * 1000);
  await signOutResponse;

  const expired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(expired.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'expired', STATE);

  await expired.signInAgainLink.click();
  await new SignInPage(page).submitCredentials(seeded.youthEmail, seeded.youthPassword);
  await expect(page).toHaveURL(new RegExp(`${reflectionUrl}$`));
  await expect(reflection.reflectionText).toHaveValue(
    'Today I learned how to listen before I speak.',
  );
  await expect(page.getByRole('status')).toContainText(/draft restored after session expiry/i);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'restored');
});
