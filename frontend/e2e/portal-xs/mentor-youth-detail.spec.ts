import { expect, test } from '@playwright/test';
import { MentorYouthDetailPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-002, L2-NFR-009, L2-NFR-010
test('assigned-youth tabs reflow and remain keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/youth/${seeded.youthId}`);

  const detail = new MentorYouthDetailPage(page);
  const progressTab = detail.tab('Progress');
  expect((await progressTab.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await progressTab.focus();
  await page.keyboard.press('Enter');
  await expect(detail.panel('Progress')).toContainText('lessons complete');
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
