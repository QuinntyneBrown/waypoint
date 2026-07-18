import { expect, test } from '@playwright/test';
import { MentorAssignWorkPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-008, L2-ENGAGE-008, L2-AUTH-010
test('mentor confirms work that reaches the youth journey, messages, and reminders', async ({
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
  await expect(assign.heading).toBeVisible();
  await assign.itemType.selectOption('Lesson');
  await assign.item.selectOption(seeded.firstLessonId);
  await assign.youthControl(seeded.youthName).click();
  await expect(assign.youthCheckbox(seeded.youthName)).toBeChecked();
  const due = new Date();
  due.setUTCDate(due.getUTCDate() + 1);
  await assign.dueDate.fill(due.toISOString().slice(0, 10));
  await assign.reminder.selectOption('2-days-before');
  const message = `Please complete this before our next session ${crypto.randomUUID()}.`;
  await assign.message.fill(message);

  await assign.review.click();
  await expect(assign.dialog).toContainText(seeded.firstLessonTitle);
  await expect(assign.dialog).toContainText('1 youth journey');
  await expect(assign.dialog).toContainText('receive your message');
  await expect(assign.dialog).toContainText('due reminder');
  await assign.confirm.click();
  await expect(assign.status).toContainText(`Assigned ${seeded.firstLessonTitle} to 1 youth`);

  const youth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const headers = { Authorization: `Bearer ${youth.accessToken}` };
  const home = await request.get('/api/me/home', { headers });
  expect(home.ok(), await home.text()).toBeTruthy();
  expect(await home.text()).toContain(seeded.firstLessonTitle);
  expect(await home.text()).toContain(message);
  const thread = await request.get('/api/me/messages', { headers });
  expect(thread.ok(), await thread.text()).toBeTruthy();
  expect(await thread.text()).toContain(message);
  const notifications = await request.get('/api/notifications', { headers });
  expect(notifications.ok(), await notifications.text()).toBeTruthy();
  const notificationBody = await notifications.text();
  expect(notificationBody).toContain('New assigned work');
  expect(notificationBody).toContain('Work due soon');
});
