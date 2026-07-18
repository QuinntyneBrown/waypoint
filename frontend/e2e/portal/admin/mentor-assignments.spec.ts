import { expect, test } from '@playwright/test';
import { MentorAssignmentsPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-ADMIN-005 AC1, AC3
test('an admin pairs a youth and the new mentor receives them immediately with updated load', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const alternateMentor = await registerUser(request, 'Mentor', 'alternate-mentor');
  const admin = await registerUser(request, 'ProgramAdministrator', 'mentor-pairing');
  await new SignInPage(page).signIn(admin);

  const assignments = new MentorAssignmentsPage(page);
  await assignments.goto();
  await assignments.youthSelect.selectOption(seeded.youthId);
  await assignments.mentorSelect.selectOption(alternateMentor.userId);
  await assignments.assignMentor.click();

  await expect(assignments.outcome).toContainText('Mentor assignment saved');
  await expect(assignments.mentorLoad(alternateMentor.userId)).toContainText('1 youth');
  await expect(assignments.mentorLoad(alternateMentor.userId)).toContainText('Under target');

  const mentorAuth = await signInApi(request, alternateMentor);
  const rosterResponse = await request.get('/api/mentor/youth', {
    headers: { Authorization: `Bearer ${mentorAuth.accessToken}` },
  });
  expect(rosterResponse.ok(), await rosterResponse.text()).toBeTruthy();
  const roster = (await rosterResponse.json()) as { id: string }[];
  expect(roster.map((item) => item.id)).toContain(seeded.youthId);
});

// Traces to: L2-ADMIN-005 AC2
test('lead and supporting mentor roles remain independent across cohorts', async ({
  page,
  request,
}) => {
  const north = await seedYouthCurriculum(request);
  const south = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'cohort-mentor-roles');
  await new SignInPage(page).signIn(admin);

  const assignments = new MentorAssignmentsPage(page);
  await assignments.goto();
  await assignments.cohortSelect.selectOption(north.cohortId);
  await expect(assignments.leadMentorSelect).toHaveValue(north.mentorId);
  await assignments.supportingMentor(south.mentorId).check();
  await assignments.saveCohortTeam.click();
  await expect(assignments.outcome).toContainText('Cohort mentor team saved');

  await assignments.cohortSelect.selectOption(south.cohortId);
  await expect(assignments.leadMentorSelect).toHaveValue(south.mentorId);
  await assignments.supportingMentor(north.mentorId).check();
  await assignments.saveCohortTeam.click();
  await expect(assignments.outcome).toContainText('Cohort mentor team saved');

  await assignments.cohortSelect.selectOption(north.cohortId);
  await expect(assignments.leadMentorSelect).toHaveValue(north.mentorId);
  await expect(assignments.supportingMentor(south.mentorId)).toBeChecked();
});
