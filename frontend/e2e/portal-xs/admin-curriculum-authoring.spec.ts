import { expect, test } from '@playwright/test';
import {
  BadgeBuilderPage,
  CurriculumBuilderPage,
  ProgressionBuilderPage,
  SignInPage,
} from '../pages';
import { registerUser, seedYouthCurriculum } from '../support/api';

// Traces to: L2-CURRIC-001..006, L2-NFR-009, L2-NFR-010
test('curriculum authoring pages remain usable on a narrow screen', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'curriculum-mobile');
  await new SignInPage(page).signIn(admin);

  const curriculum = new CurriculumBuilderPage(page);
  await curriculum.goto();
  await curriculum.trackSelect.selectOption(seeded.primaryTrackId);
  await expect(curriculum.module(seeded.firstModuleName)).toBeVisible();
  expect((await curriculum.newTrack.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => curriculum.horizontalOverflow()).toBeLessThanOrEqual(1);

  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();
  expect((await badges.saveBadge.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => badges.horizontalOverflow()).toBeLessThanOrEqual(1);

  const progression = new ProgressionBuilderPage(page);
  await progression.goto();
  await expect(progression.heading).toBeVisible();
  expect((await progression.addStage.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  await expect.poll(() => progression.horizontalOverflow()).toBeLessThanOrEqual(1);
});
