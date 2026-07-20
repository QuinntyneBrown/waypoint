import { expect, test } from '@playwright/test';
import { MentorCompetencyPage, SignInPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-MENTOR-005, L2-NFR-009, L2-NFR-010
test('competency assessment reflows and saves by keyboard on mobile', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto(`/mentor/youth/${seeded.youthId}/competencies`);

  const tracker = new MentorCompetencyPage(page);
  await tracker.levelById(seeded.mentorCompetencyId).focus();
  await page.keyboard.press('End');
  await expect(tracker.levelById(seeded.mentorCompetencyId)).toHaveValue('Mastered');
  await tracker.notesById(seeded.mentorCompetencyId).focus();
  await page.keyboard.type('Assessed from a phone-sized viewport.');
  await tracker.saveById(seeded.mentorCompetencyId).focus();
  await page.keyboard.press('Enter');
  await expect(tracker.status).toContainText(`Saved ${seeded.mentorCompetencyName}`);
  expect(
    (await tracker.saveById(seeded.mentorCompetencyId).boundingBox())?.height ?? 0,
  ).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
