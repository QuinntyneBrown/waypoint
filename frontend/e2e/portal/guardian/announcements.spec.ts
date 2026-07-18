import { expect, test } from '@playwright/test';
import { GuardianAnnouncementsPage, SignInPage } from '../../pages';
import { registerUser, seedGuardianFamily, signInApi } from '../../support/api';

// Traces to: L2-GUARDIAN-002
test('guardian news is correctly targeted and mark-all-read clears unread state', async ({
  page,
  request,
}) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-news');
  const family = await seedGuardianFamily(request, guardian.userId);
  const administrator = await registerUser(request, 'ProgramAdministrator', 'news-admin');
  const adminAuth = await signInApi(request, administrator);
  const visibleTitle = `Guardian cohort update ${crypto.randomUUID()}`;
  const excludedTitle = `Youth-only update ${crypto.randomUUID()}`;

  for (const announcement of [
    { title: visibleTitle, roleTargeting: 'Guardians' },
    { title: excludedTitle, roleTargeting: 'Youth' },
  ]) {
    const response = await request.post('/api/announcements', {
      headers: { Authorization: `Bearer ${adminAuth.accessToken}` },
      data: {
        title: announcement.title,
        message: `${announcement.title} body`,
        audienceScope: 'cohort',
        cohortId: family.cohortId,
        roleTargeting: announcement.roleTargeting,
        scheduledAt: null,
      },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
  }

  await new SignInPage(page).signIn(guardian);
  const news = new GuardianAnnouncementsPage(page);
  await news.goto();

  await expect(news.heading).toBeVisible();
  await expect(news.announcements.filter({ hasText: visibleTitle })).toBeVisible();
  await expect(page.getByText(excludedTitle)).toHaveCount(0);
  await expect(news.unreadAnnouncements).toHaveCount(1);

  await news.markAllReadButton.click();
  await expect(news.unreadAnnouncements).toHaveCount(0);
});

test('guardian news has an explicit empty state', async ({ page, request }) => {
  const guardian = await registerUser(request, 'Guardian', 'guardian-news-empty');
  await new SignInPage(page).signIn(guardian);
  const news = new GuardianAnnouncementsPage(page);
  await news.goto();

  await expect(page.getByRole('heading', { name: /no announcements yet/i })).toBeVisible();
});
