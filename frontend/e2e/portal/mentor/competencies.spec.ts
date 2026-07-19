import { expect, test } from '@playwright/test';
import { MentorCompetencyPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-005, L2-LEARN-010
test('mentor assesses the full competency framework and youth progress gets the latest value', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/youth/${seeded.youthId}/competencies`);

  const tracker = new MentorCompetencyPage(page);
  await expect(tracker.heading).toContainText(seeded.youthName);
  await expect(tracker.summary).toContainText('1 of 2');
  await expect(tracker.summary).toContainText(/Current stage.+\d+%/s);
  await expect(tracker.level(seeded.mentorCompetencyName)).toHaveValue('NotStarted');
  await tracker.level(seeded.mentorCompetencyName).selectOption('Proficient');
  await tracker
    .notes(seeded.mentorCompetencyName)
    .fill('Took initiative welcoming a new participant.');
  await tracker.save(seeded.mentorCompetencyName).click();
  await expect(tracker.status).toContainText(`Saved ${seeded.mentorCompetencyName}`);
  await expect(tracker.summary).toContainText('2 of 2');
  await expect(tracker.card(seeded.mentorCompetencyName)).toContainText(seeded.mentorName);

  await page.reload();
  await expect(tracker.level(seeded.mentorCompetencyName)).toHaveValue('Proficient');
  await expect(tracker.notes(seeded.mentorCompetencyName)).toHaveValue(
    'Took initiative welcoming a new participant.',
  );

  const youth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const progressResponse = await request.get('/api/me/progress', {
    headers: { Authorization: `Bearer ${youth.accessToken}` },
  });
  expect(progressResponse.ok(), await progressResponse.text()).toBeTruthy();
  const progress = (await progressResponse.json()) as {
    competencies: { competencyId: string; name: string; level: string }[];
  };
  expect(progress.competencies).toContainEqual(
    expect.objectContaining({
      competencyId: seeded.mentorCompetencyId,
      name: seeded.mentorCompetencyName,
      level: 'Proficient',
    }),
  );
});
