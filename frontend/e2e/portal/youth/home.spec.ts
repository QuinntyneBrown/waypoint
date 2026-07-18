import { expect, test } from '@playwright/test';
import { SignInPage, YouthHomePage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-001..010, L2-ENGAGE-005, L2-MENTOR-008
test('placed youth home combines learning, progress, schedule, assignments, and mentor support', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const home = new YouthHomePage(page);
  await expect(home.placedDashboard).toBeVisible();
  await expect(home.placedDashboard).toContainText(seeded.firstModuleName);
  await expect(home.continueLearningLink).toBeVisible();
  await expect(home.progressCard).toContainText('1 of 3 required items complete');
  await expect(home.progressCard).toContainText('33%');
  await expect(home.sessionCard).toContainText(seeded.scheduleSessionTitle);
  await expect(home.sessionCard).toContainText(seeded.scheduleSessionLocation);
  await expect(home.assignmentsCard).toContainText(seeded.assignmentTitle);
  await expect(home.assignmentsCard).toContainText('Bring your leadership ideas to this project.');
  await expect(home.assignmentsCard).toContainText(seeded.mentorName);
});
