import { expect, test } from '@playwright/test';
import { MentorAssignWorkPage, SignInPage, YouthHomePage } from '../../e2e/pages';
import { seedCast } from '../mentoring-cast';
import { shot } from '../shots';

const FOLDER = 'journeys/20-assign-work';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: journey 20 assign work', async ({ page, request, browser }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto('/mentor/assign-work');

  const assign = new MentorAssignWorkPage(page);
  await expect(assign.heading).toBeVisible();
  await assign.itemType.selectOption('Lesson');
  await assign.item.selectOption(seeded.secondLessonId);
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
  await expect(assign.status).toContainText(`Assigned ${seeded.secondLessonTitle} to 1 youth`);
  await assign.status.scrollIntoViewIfNeeded();
  await page.waitForTimeout(500);
  await shot(page, FOLDER, 'assigned', MAIN);

  const youthPage = await browser.newPage({
    baseURL: 'http://localhost:4200',
    viewport: { width: 1280, height: 800 },
  });
  await new SignInPage(youthPage).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await expect(youthPage).toHaveURL(YouthHomePage.urlPattern);
  const card = youthPage.getByTestId('home-assignments');
  await expect(card).toContainText(seeded.secondLessonTitle);
  await youthPage.waitForLoadState('networkidle');
  await card.scrollIntoViewIfNeeded();
  await youthPage.waitForTimeout(500);
  await shot(youthPage, FOLDER, 'youth-home');
});
