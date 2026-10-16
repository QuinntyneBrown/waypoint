import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '046-curriculum-authoring-author-lessons';

test('capture: author lessons', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await signInAsPat(page, request);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  const module = builder.module(seeded.firstModuleName);
  await expect(module).toBeVisible();

  const title = 'Serving like Jesus ' + crypto.randomUUID().slice(0, 6);
  await module.getByRole('button', { name: 'Add lesson' }).click();
  const dialog = builder.lessonDialog;
  await dialog.getByLabel('Lesson title').fill(title);
  await dialog.getByLabel('Scripture reference').fill('Mark 10:45');
  await dialog.getByLabel('Estimated minutes').fill('25');
  await dialog.getByLabel('Learning objectives').fill('Name one way Jesus served others.');
  await dialog
    .getByLabel('Teaching body')
    .fill('Jesus came to serve, not to be served. Look at how he treated people around him.');
  await dialog.getByLabel('Reflection prompts').fill('Who can you serve this week?');
  await dialog
    .getByLabel('Leader guide notes')
    .fill('Invite safe, everyday examples. Keep the discussion short.');
  await shotOf(dialog, FOLDER, 'dialog');

  await dialog.getByRole('button', { name: 'Create lesson' }).click();
  const lesson = builder.lesson(title);
  await expect(lesson).toBeVisible();
  await lesson.getByLabel('Upload PDF or MP4').setInputFiles({
    name: 'leader-guide.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.7\nWaypoint curriculum guide'),
  });
  await expect(lesson).toContainText('leader-guide.pdf');
  await shotOf(lesson, FOLDER, 'media');

  await lesson.getByRole('button', { name: 'Publish lesson' }).click();
  await expect(builder.outcome).toContainText('Lesson published.');
  await expect(lesson).toContainText('Published');
  await shotOf(builder.outcome, FOLDER, 'published', 12);
  await shotOf(lesson, FOLDER, 'card');
});
