import { expect, test } from '@playwright/test';
import { MentorCheckInsPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-006, L2-LEARN-010, L2-GUARDIAN-001
test('mentor logs and filters a shared check-in that reaches youth and guardian', async ({
  browser,
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/check-ins?youthId=${seeded.youthId}`);

  const journal = new MentorCheckInsPage(page);
  await expect(journal.heading).toBeVisible();
  await expect(journal.youth).toHaveValue(seeded.youthId);
  await expect(journal.share).not.toBeChecked();
  await expect(journal.historyItems).toContainText('Private mentor note');
  await expect(journal.historyItems).toContainText('PRIVATE PRAYER DETAIL');

  await journal.type.selectOption('call');
  await journal.engagement.fill('Hopeful and ready to lead.');
  await journal.goals.fill('Welcome the next new participant.');
  await journal.prayer.fill('Shared prayer for courage.');
  await journal.actions.fill('Draft a welcome message.');
  await journal.shareControl.click();
  await expect(page.getByText(/including prayer requests/i)).toBeVisible();
  await journal.guardianInformedControl.click();
  await journal.save.click();
  await expect(journal.status).toContainText('Check-in saved');
  const sharedCheckIn = journal.historyItems.filter({ hasText: 'Shared prayer for courage.' });
  await expect(sharedCheckIn).toHaveCount(1);
  await expect(sharedCheckIn).toContainText('Guardian was informed');

  await journal.filterType.selectOption('call');
  await expect(journal.historyItems).toHaveCount(1);

  const youthContext = await browser.newContext({ baseURL: 'http://localhost:4200' });
  const youthPage = await youthContext.newPage();
  await new SignInPage(youthPage).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await youthPage.goto('/youth/progress');
  await expect(youthPage.getByTestId('youth-shared-checkins')).toContainText(
    'Shared prayer for courage.',
  );
  await youthContext.close();

  const guardian = await signInApi(request, {
    email: seeded.guardianEmail,
    password: seeded.youthPassword,
  });
  const guardianProgress = await request.get(`/api/guardian/children/${seeded.youthId}/progress`, {
    headers: { Authorization: `Bearer ${guardian.accessToken}` },
  });
  expect(guardianProgress.ok(), await guardianProgress.text()).toBeTruthy();
  expect(await guardianProgress.text()).toContain('Shared prayer for courage.');
});
