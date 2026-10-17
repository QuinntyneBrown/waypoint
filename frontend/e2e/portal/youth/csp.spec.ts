import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage, YouthLessonPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';
import { seedLessonVideo } from '../../support/video';

// Traces to: L2-NFR-002 AC3, L2-LEARN-002 AC5
test('the portal declares a content security policy that still lets a lesson video stream', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await seedLessonVideo(request, seeded.secondLessonId, { ready: true });
  const lesson = new YouthLessonPage(page);
  await lesson.recordPlayerRendering();
  await lesson.forceHlsJsAndRecordMediaSources();
  await lesson.recordCspViolations();

  await new SignInPage(page).signIn({ email: seeded.youthEmail, password: seeded.youthPassword });
  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await curriculum.module(seeded.firstModuleName).getByText(seeded.secondLessonTitle).click();

  await expect.poll(() => lesson.playerWasRendered()).toBe(true);
  await expect.poll(async () => (await lesson.mediaSourceUrls()).length).toBeGreaterThan(0);
  const policy = await lesson.contentSecurityPolicy();
  expect(policy).toContain("media-src 'self' blob:");
  expect(policy).toContain("connect-src 'self'");
  expect(policy).toContain("img-src 'self'");
  // The seeded lesson also carries an external demo video address, which the policy rightly blocks.
  const violations = await lesson.cspViolations();
  expect(violations.filter((violation) => !violation.includes('media.example.test'))).toEqual([]);
});
