import { expect, test } from '@playwright/test';
import { MentorCheckInsPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-006, L2-NFR-009, L2-NFR-010
test('check-in journal is contained and keyboard savable on mobile', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/check-ins?youthId=${seeded.youthId}`);

  const journal = new MentorCheckInsPage(page);
  await journal.engagement.focus();
  await page.keyboard.type('Mobile check-in note.');
  await journal.save.focus();
  await page.keyboard.press('Enter');
  await expect(journal.status).toContainText('Check-in saved');
  expect((await journal.save.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
