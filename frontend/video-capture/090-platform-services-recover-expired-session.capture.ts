import { expect, test } from '@playwright/test';
import { SessionExpiredPage, SignInPage, YouthReflectionPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '090-platform-services-recover-expired-session';
const STATE = { x: 340, y: 230, width: 600, height: 340 };
const DRAFT_TEXT = 'Today I learned how to listen before I speak.';

test('capture: expired session', async ({ page, request }) => {
  await page.clock.install();
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const reflectionUrl = `/youth/lessons/${seeded.secondLessonId}/reflection`;
  await page.goto(reflectionUrl);

  const reflection = new YouthReflectionPage(page);
  await reflection.reflectionText.fill(DRAFT_TEXT);
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'draft');

  await page.clock.fastForward(20 * 60 * 1000);
  await page.keyboard.press('Shift');
  await page.clock.fastForward(20 * 60 * 1000);
  const signedOut = page.waitForResponse(
    (response) => response.url().includes('/api/auth/sign-out') && response.status() === 204,
    { timeout: 5_000 },
  );
  await page.clock.fastForward(11 * 60 * 1000);
  await signedOut;

  const expired = new SessionExpiredPage(page);
  await expect(page).toHaveURL(SessionExpiredPage.urlPattern);
  await expect(expired.heading).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'expired', STATE);

  await expired.signInAgainLink.click();
  await new SignInPage(page).submitCredentials(seeded.youthEmail, seeded.youthPassword);
  await expect(page).toHaveURL(new RegExp(`${reflectionUrl}$`));
  await expect(reflection.reflectionText).toHaveValue(DRAFT_TEXT);
  await expect(page.getByRole('status')).toContainText(/draft restored after session expiry/i);
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'restored');
});
