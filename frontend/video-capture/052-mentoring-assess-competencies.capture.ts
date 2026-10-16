import { expect, test } from '@playwright/test';
import { MentorCompetencyPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';
import { shotOf } from './shotOf';

const FOLDER = '052-mentoring-assess-competencies';

test('capture: assess competencies', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'Amara Bell');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto(`/mentor/youth/${seeded.youthId}/competencies`);

  const tracker = new MentorCompetencyPage(page);
  await expect(tracker.heading).toContainText('Amara Bell');
  await expect(tracker.summary).toContainText(/Proficient or mastered/);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'tracker', { x: 250, y: 70, width: 1030, height: 400 });

  const id = seeded.mentorCompetencyId;
  await tracker.levelById(id).scrollIntoViewIfNeeded();
  await tracker.levelById(id).selectOption('Proficient');
  await tracker.notesById(id).fill('Took initiative welcoming a new participant.');
  await shotOf(page, FOLDER, 'card', tracker.cardById(id));

  await tracker.saveById(id).click();
  await expect(tracker.status).toContainText(`Saved ${seeded.mentorCompetencyName}`);
  await expect(tracker.cardById(id)).toContainText('Grace Adeyemi');
  await shotOf(page, FOLDER, 'saved', tracker.cardById(id));

  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'summary', { x: 250, y: 70, width: 1030, height: 400 });
});
