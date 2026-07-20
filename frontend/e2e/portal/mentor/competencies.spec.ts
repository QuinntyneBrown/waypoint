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
  const initialSummary = (await tracker.summary.textContent()) ?? '';
  const initialCounts = initialSummary.match(/Proficient or mastered\s*(\d+) of (\d+)/);
  expect(initialCounts, initialSummary).not.toBeNull();
  const initiallyMastered = Number(initialCounts![1]);
  const frameworkTotal = Number(initialCounts![2]);
  await expect(tracker.summary).toContainText(/Current stage.+\d+%/s);
  await expect(tracker.levelById(seeded.mentorCompetencyId)).toHaveValue('NotStarted');
  await tracker.levelById(seeded.mentorCompetencyId).selectOption('Proficient');
  await tracker
    .notesById(seeded.mentorCompetencyId)
    .fill('Took initiative welcoming a new participant.');
  await tracker.saveById(seeded.mentorCompetencyId).click();
  await expect(tracker.status).toContainText(`Saved ${seeded.mentorCompetencyName}`);
  await expect(tracker.summary).toContainText(
    new RegExp(`Proficient or mastered\\s*${initiallyMastered + 1} of \\d+`),
  );
  const updatedSummary = (await tracker.summary.textContent()) ?? '';
  const updatedCounts = updatedSummary.match(/Proficient or mastered\s*(\d+) of (\d+)/);
  expect(updatedCounts, updatedSummary).not.toBeNull();
  expect(Number(updatedCounts![2])).toBeGreaterThanOrEqual(frameworkTotal);
  await expect(tracker.cardById(seeded.mentorCompetencyId)).toContainText(seeded.mentorName);

  await page.reload();
  await expect(tracker.levelById(seeded.mentorCompetencyId)).toHaveValue('Proficient');
  await expect(tracker.notesById(seeded.mentorCompetencyId)).toHaveValue(
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
