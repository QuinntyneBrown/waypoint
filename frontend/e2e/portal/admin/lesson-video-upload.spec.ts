import { expect, test } from '@playwright/test';
import { CurriculumBuilderPage, SignInPage } from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';
import { fakeMp4, runVideoTranscodes } from '../../support/video';

const MIB = 1024 * 1024;

async function openLesson(
  page: import('@playwright/test').Page,
  request: import('@playwright/test').APIRequestContext,
) {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'lesson-video');
  await new SignInPage(page).signIn(admin);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  const lesson = builder.lesson(seeded.firstLessonTitle);
  await expect(lesson).toBeVisible();
  return { builder, lesson };
}

// Traces to: L2-CURRIC-002 AC6, AC7
test('an admin uploads a video in chunks and sees it move from queued to ready', async ({
  page,
  request,
}) => {
  const { builder, lesson } = await openLesson(page, request);

  await builder.attachVideo(lesson, { name: 'week-1.mp4', buffer: fakeMp4(9 * MIB) });

  const video = builder.video(lesson, 'week-1.mp4');
  await expect(video).toContainText('Queued');
  await runVideoTranscodes(request);
  await expect(video).toContainText('Ready', { timeout: 20_000 });
  await expect(video).toContainText('2:00');
});

// Traces to: L2-CURRIC-002 AC6, L2-NFR-001 AC4
test('a file that is not a video is rejected and nothing is attached', async ({
  page,
  request,
}) => {
  const { builder, lesson } = await openLesson(page, request);

  await builder.attachVideo(lesson, {
    name: 'schedule.mp4',
    buffer: fakeMp4(2 * MIB, 'NOTVIDEO'),
  });

  await expect(builder.videoUpload(lesson, 'schedule.mp4')).toContainText(
    'This file is not a video we can read.',
  );
  await expect(builder.video(lesson, 'schedule.mp4')).toHaveCount(0);
});

// Traces to: L2-CURRIC-002 AC7
test('a failed transcode shows as failed and can be retried', async ({ page, request }) => {
  const { builder, lesson } = await openLesson(page, request);
  await builder.attachVideo(lesson, {
    name: 'broken.mp4',
    buffer: fakeMp4(2 * MIB, 'FAILTRANSCODE'),
  });
  const video = builder.video(lesson, 'broken.mp4');
  await expect(video).toContainText('Queued');

  await runVideoTranscodes(request);
  await runVideoTranscodes(request);
  await expect(video).toContainText('Failed', { timeout: 20_000 });
  await builder.videoRetryButton(video).click();

  await expect(video).toContainText('Queued');
});
