import { expect, test } from '@playwright/test';
import { MentorFlagsPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-007, L2-NFR-009, L2-NFR-010
test('at-risk actions remain keyboard usable and contained on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/flags');

  const atRisk = new MentorFlagsPage(page);
  const flag = atRisk.flag(seeded.youthName);
  const messageButton = flag.getByRole('button', { name: /^message$/i });
  expect((await messageButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await messageButton.focus();
  await page.keyboard.press('Enter');
  const message = page.getByLabel(`Message ${seeded.youthName}`);
  await message.focus();
  await page.keyboard.type('Mobile safeguarding check-in.');
  await page.getByRole('button', { name: /send message/i }).focus();
  await page.keyboard.press('Enter');
  await expect(atRisk.status).toContainText(`Message sent to ${seeded.youthName}`);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
