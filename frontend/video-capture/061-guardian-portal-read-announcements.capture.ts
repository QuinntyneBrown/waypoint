import { expect, test } from '@playwright/test';
import { GuardianAnnouncementsPage, SignInPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { seedGuardianCast } from './guardian-cast';
import { shot } from './shots';

const LIST = { x: 400, y: 85, width: 760, height: 510 };

const FOLDER = '061-guardian-portal-read-announcements';

test('capture: read announcements', async ({ page, request, browser }) => {
  test.setTimeout(120_000);
  const { guardian, family } = await seedGuardianCast(request, 'Imani Clarke');
  const adminEmail = `pat-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const created = await request.post('/api/_test/users', {
    data: {
      email: adminEmail,
      password: 'Password123',
      displayName: 'Pat Morgan',
      role: 'ProgramAdministrator',
    },
  });
  expect(created.ok()).toBeTruthy();
  const admin = await signInApi(request, { email: adminEmail, password: 'Password123' });
  const post = async (title: string, message: string, roleTargeting: string) => {
    const res = await request.post('/api/announcements', {
      headers: { Authorization: `Bearer ${admin.accessToken}` },
      data: {
        title,
        message,
        audienceScope: 'cohort',
        cohortId: family.cohortId,
        roleTargeting,
        scheduledAt: null,
      },
    });
    expect(res.ok(), await res.text()).toBeTruthy();
  };
  await post(
    'Family showcase this Friday',
    'Doors open at six. Families are welcome to stay for refreshments after the showcase.',
    'Guardians',
  );
  await post(
    'Pick-up arrangements for next week',
    'Next week the session ends at eight. Please collect young people from the main hall.',
    'Guardians',
  );
  await post('Youth: bring your laptops', 'This message is for youth only.', 'Youth');

  await new SignInPage(page).signIn(guardian);
  const news = new GuardianAnnouncementsPage(page);
  await news.goto();
  await expect(news.unreadAnnouncements).toHaveCount(2);
  await expect(page.getByText('Youth: bring your laptops')).toHaveCount(0);
  await page.setViewportSize({ width: 1280, height: 800 });
  await shot(page, FOLDER, 'unread', LIST);

  await news.markAllReadButton.click();
  await expect(news.unreadAnnouncements).toHaveCount(0);
  await shot(page, FOLDER, 'read', LIST);

  const other = await (await browser.newContext({ baseURL: 'http://localhost:4200' })).newPage();
  const emptyEmail = `ruth-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  await request.post('/api/_test/users', {
    data: {
      email: emptyEmail,
      password: 'Password123',
      displayName: 'Ruth Clarke',
      role: 'Guardian',
    },
  });
  await new SignInPage(other).signIn({ email: emptyEmail, password: 'Password123' });
  await new GuardianAnnouncementsPage(other).goto();
  await expect(other.getByRole('heading', { name: /no announcements yet/i })).toBeVisible();
  const title = (await other.locator('.ph-empty__title').boundingBox())!;
  await shot(other, FOLDER, 'empty', {
    x: title.x + title.width / 2 - 250,
    y: title.y - 120,
    width: 500,
    height: 230,
  });
});
