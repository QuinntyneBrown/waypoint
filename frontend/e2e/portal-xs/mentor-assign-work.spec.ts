import { expect, test } from '@playwright/test';
import { MentorAssignWorkPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-008, L2-NFR-009, L2-NFR-010
test('assign-work confirmation is keyboard usable and contained on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/assign-work');

  const assign = new MentorAssignWorkPage(page);
  await assign.item.selectOption(seeded.firstLessonId);
  await assign.youthControl(seeded.youthName).click();
  await assign.message.focus();
  await page.keyboard.type('Mobile journey assignment.');
  await assign.review.focus();
  await page.keyboard.press('Enter');
  await expect(assign.dialog).toBeVisible();
  await expect(assign.dialog.getByRole('button', { name: 'Cancel', exact: true })).toBeFocused();
  expect((await assign.confirm.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await page.keyboard.press('Tab');
  await expect(assign.confirm).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(assign.status).toContainText('Assigned');
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
