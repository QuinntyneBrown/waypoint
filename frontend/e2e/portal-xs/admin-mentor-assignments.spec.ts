import { expect, test } from '@playwright/test';
import { MentorAssignmentsPage, SignInPage } from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-ADMIN-005, L2-NFR-009, L2-NFR-010
test('mentor assignment controls and load cards reflow without narrow-screen overflow', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'mentor-mobile');
  await new SignInPage(page).signIn(admin);

  const assignments = new MentorAssignmentsPage(page);
  await assignments.goto();
  await assignments.youthSelect.selectOption(seeded.youthId);
  await assignments.mentorSelect.selectOption(seeded.mentorId);
  await expect(assignments.mentorLoad(seeded.mentorId)).toBeVisible();
  expect((await assignments.assignMentor.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => assignments.horizontalOverflow()).toBeLessThanOrEqual(1);
});
