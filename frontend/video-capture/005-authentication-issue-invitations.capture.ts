import { Page, expect, test } from '@playwright/test';
import { ForbiddenPage, InvitationsPage, SignInPage } from '../e2e/pages';
import { registerYouth, seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '005-authentication-issue-invitations';

async function zoomOut(page: Page): Promise<void> {
  await page.evaluate(() => {
    document.documentElement.style.zoom = '0.7';
    window.scrollTo(0, 0);
  });
  await page.waitForTimeout(300);
}

async function firstCardClip(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
  const box = await page.locator('.w-card').first().boundingBox();
  expect(box).not.toBeNull();
  return { x: box!.x - 12, y: box!.y - 12, width: box!.width + 24, height: box!.height + 24 };
}

test('capture: issue invitations', async ({ page, request, browser }) => {
  const seeded = await seedYouthCurriculum(request);
  const patEmail = `pat-${crypto.randomUUID()}@waypoint.demo`;
  const created = await request.post('/api/_test/users', {
    data: {
      email: patEmail,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(created.ok()).toBeTruthy();

  await page.setViewportSize({ width: 1280, height: 800 });
  await new SignInPage(page).signIn({ email: patEmail, password: 'Password123' });
  const invitations = new InvitationsPage(page);
  await invitations.goto();
  await expect(invitations.sendButton).toBeVisible();
  await page.waitForLoadState('networkidle');

  const tag = crypto.randomUUID().slice(0, 8);
  await page.getByLabel('Role', { exact: true }).first().selectOption('Youth');
  await page.getByLabel('Cohort (optional)').selectOption(seeded.cohortId);
  await page.getByLabel('Mentor (optional)').selectOption(seeded.mentorId);
  await invitations.emails.fill(
    [1, 2, 3].map((n) => `intake-${tag}-${n}@waypoint.demo`).join('\n'),
  );
  await zoomOut(page);
  await shot(page, FOLDER, 'form', await firstCardClip(page));

  await invitations.sendButton.click();
  await expect(page.getByText('3 invitations sent.', { exact: true })).toBeVisible();
  await page.getByLabel('Search invitations').fill(`intake-${tag}`);
  await page.getByRole('button', { name: 'Apply search' }).click();
  await expect(invitations.rows).toHaveCount(3);
  await page.mouse.move(0, 0);
  await zoomOut(page);
  await shot(page, FOLDER, 'sent');

  // A mentor can invite youth into their own scope, nothing else.
  const mentorPage = await browser.newPage({
    baseURL: 'http://localhost:4200',
    viewport: { width: 1280, height: 800 },
  });
  await new SignInPage(mentorPage).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await mentorPage.goto('/mentor/invitations');
  await expect(
    mentorPage.getByText('Invitations you send create Youth accounts assigned to you.'),
  ).toBeVisible();
  await mentorPage.waitForLoadState('networkidle');
  await zoomOut(mentorPage);
  await shot(mentorPage, FOLDER, 'mentor');

  // A youth cannot reach the screen at all.
  const youth = await registerYouth(request, 'denied');
  const youthPage = await browser.newPage({
    baseURL: 'http://localhost:4200',
    viewport: { width: 1280, height: 800 },
  });
  await new SignInPage(youthPage).signIn(youth);
  await youthPage.goto('/admin/invitations');
  await expect(new ForbiddenPage(youthPage).heading).toBeVisible();
  await youthPage.waitForLoadState('networkidle');
  await shot(youthPage, FOLDER, 'forbidden');
});
