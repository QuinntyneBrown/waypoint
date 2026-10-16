import { APIRequestContext, expect, test } from '@playwright/test';
import {
  GuardianHomePage,
  GuardianProgressPage,
  MentorDashboardPage,
  SignInPage,
  VolunteerTodayPage,
  YouthHomePage,
} from '../e2e/pages';
import { seedGuardianFamily, seedVolunteerSession, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '011-authentication-scope-data-by-role';

async function seedCast(request: APIRequestContext, role: string, displayName: string) {
  const email = `${displayName.split(' ')[0].toLowerCase()}-${crypto.randomUUID().slice(0, 6)}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName, role },
  });
  expect(res.ok()).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  return { email, password, userId };
}

test('capture: scope data by role', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  const dashboard = new MentorDashboardPage(page);
  await expect(dashboard.heading).toBeVisible();
  await expect(dashboard.rosterPreview).toContainText(seeded.youthName);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'mentor', { x: 0, y: 0, width: 1280, height: 590 });

  // A youth who opens a mentor address is not shown mentor data.
  const youthPage = await page.context().browser()!.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(youthPage).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await expect(youthPage).toHaveURL(YouthHomePage.urlPattern);
  await youthPage.goto('/mentor/dashboard');
  await youthPage.waitForLoadState('networkidle');
  await shot(youthPage, FOLDER, 'blocked');

  const guardian = await seedCast(request, 'Guardian', 'Ruth Clarke');
  const family = await seedGuardianFamily(request, guardian.userId);
  const guardianPage = await page
    .context()
    .browser()!
    .newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(guardianPage).signIn(guardian);
  await expect(guardianPage).toHaveURL(GuardianHomePage.urlPattern);
  await expect(new GuardianHomePage(guardianPage).childName).toContainText(family.childName);
  await guardianPage.waitForLoadState('networkidle');
  await shot(guardianPage, FOLDER, 'guardian');
  void GuardianProgressPage;

  const volunteer = await seedCast(request, 'Volunteer', 'Sam Whitfield');
  await seedVolunteerSession(request, volunteer.userId);
  const volPage = await page.context().browser()!.newPage({ baseURL: 'http://localhost:4200' });
  await new SignInPage(volPage).signIn(volunteer);
  await expect(volPage).toHaveURL(VolunteerTodayPage.urlPattern);
  await expect(new VolunteerTodayPage(volPage).heading).toBeVisible();
  await volPage.waitForLoadState('networkidle');
  await shot(volPage, FOLDER, 'volunteer');
});
