import { expect, test } from '@playwright/test';
import { MentorAssignWorkPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';

const FOLDER = '053-mentoring-assign-work';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: assign work', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto('/mentor/assign-work');

  const assign = new MentorAssignWorkPage(page);
  await expect(assign.heading).toBeVisible();
  await assign.itemType.selectOption('Lesson');
  await assign.item.selectOption(seeded.firstLessonId);
  await expect(assign.youthControl('Amara Bell')).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'form', MAIN);

  await assign.youthControl('Amara Bell').click();
  const due = new Date();
  due.setUTCDate(due.getUTCDate() + 7);
  await assign.dueDate.fill(due.toISOString().slice(0, 10));
  await assign.reminder.selectOption('2-days-before');
  await assign.message.fill('Please complete this before our next session.');
  await shot(page, FOLDER, 'filled', MAIN);

  await assign.review.click();
  await expect(assign.dialog).toContainText('1 youth journey');
  await page.waitForTimeout(800);
  await shot(page, FOLDER, 'confirm');

  await assign.confirm.click();
  await expect(assign.status).toContainText(`Assigned ${seeded.firstLessonTitle} to 1 youth`);
});
