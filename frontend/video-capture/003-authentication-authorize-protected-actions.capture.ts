import { expect, test } from '@playwright/test';
import { ForbiddenPage, SignInPage } from '../e2e/pages';
import { registerYouth, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '003-authentication-authorize-protected-actions';

test('capture: authorize protected actions', async ({ page, request, browser }) => {
  // No sign-in: a protected page sends the visitor to sign in.
  await page.goto('/admin/people');
  await expect(page).toHaveURL(/\/auth\/sign-in/);
  await expect(new SignInPage(page).submitButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'signed-out', { x: 380, y: 90, width: 520, height: 620 });

  // A youth reaching for an administrator screen is turned away.
  const youth = await registerYouth(request, 'amara');
  const youthPage = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(youthPage).signIn(youth);
  await youthPage.goto('/admin/people');
  await expect(new ForbiddenPage(youthPage).heading).toBeVisible();
  await youthPage.waitForLoadState('networkidle');
  await shot(youthPage, FOLDER, 'forbidden', { x: 340, y: 230, width: 600, height: 340 });

  // A mentor who is not assigned to a youth cannot open that youth's record, even with the id.
  const seeded = await seedYouthCurriculum(request);
  const otherEmail = `grace-${crypto.randomUUID()}@waypoint.demo`;
  const other = await request.post('/api/_test/users', {
    data: {
      email: otherEmail,
      password: 'Password123',
      displayName: 'Grace Adeyemi',
      role: 'Mentor',
    },
  });
  expect(other.ok()).toBeTruthy();
  const mentorPage = await browser.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(mentorPage).signIn({ email: otherEmail, password: 'Password123' });
  await mentorPage.goto(`/mentor/youth/${seeded.youthId}`);
  await mentorPage.waitForTimeout(2000);
  await mentorPage.waitForLoadState('networkidle');
  await shot(mentorPage, FOLDER, 'mentor-denied', { x: 340, y: 230, width: 600, height: 340 });
});
