import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage, YouthLessonPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';
import { seedLessonVideo } from '../../support/video';

async function openSecondLesson(
  page: import('@playwright/test').Page,
  seeded: Awaited<ReturnType<typeof seedYouthCurriculum>>,
) {
  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await curriculum.module(seeded.firstModuleName).getByText(seeded.secondLessonTitle).click();
}

// Traces to: L2-LEARN-002 AC5, AC6
test('a youth plays a ready lesson video from a signed adaptive stream', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await seedLessonVideo(request, seeded.secondLessonId, { ready: true });
  const lesson = new YouthLessonPage(page);
  await lesson.recordPlayerRendering();
  const master = lesson.waitForMasterPlaylist();

  await openSecondLesson(page, seeded);

  // A range response (206) is as good as a full one: browsers that play HLS natively ask for ranges.
  expect((await master).ok()).toBe(true);
  await expect.poll(() => lesson.playerWasRendered()).toBe(true);
  await expect(lesson.videoPreparingNotice).toHaveCount(0);
});

// Traces to: L2-LEARN-002 AC1
test('a youth sees a preparing notice, not a player, while the video is queued', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await seedLessonVideo(request, seeded.secondLessonId, { ready: false });

  await openSecondLesson(page, seeded);

  const lesson = new YouthLessonPage(page);
  await expect(lesson.videoPreparingNotice).toBeVisible();
  await expect(lesson.videoPlayer).toHaveCount(0);
});
