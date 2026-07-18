import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-001, L2-NFR-009, L2-NFR-010
test('the curriculum map reflows without overflow and track switching is keyboard operable', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();

  await expect(curriculum.module(seeded.firstModuleName)).toBeVisible();
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);

  const secondTrack = curriculum.trackOptions.filter({ hasText: seeded.secondTrackName });
  const box = await secondTrack.boundingBox();
  expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);
  await secondTrack.focus();
  await page.keyboard.press('Enter');
  await expect(curriculum.emptyState).toBeVisible();
});
