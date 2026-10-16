import { expect, test } from '@playwright/test';
import { SignInPage, YouthCurriculumPage } from '../e2e/pages';
import { seedYouthCurriculum } from '../e2e/support/api';
import { shot } from './shots';

const FOLDER = '032-learning-experience-browse-curriculum';

test('capture: browse curriculum', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });

  const curriculum = new YouthCurriculumPage(page);
  await curriculum.goto();
  await expect(curriculum.modules).toHaveCount(2);
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'map');

  await curriculum.module(seeded.firstModuleName).getByText('Module quiz').scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'items');

  await curriculum.module(seeded.lockedModuleName).scrollIntoViewIfNeeded();
  await shot(page, FOLDER, 'locked');

  await curriculum.trackOptions.filter({ hasText: seeded.secondTrackName }).click();
  await expect(curriculum.emptyState).toBeVisible();
  await shot(page, FOLDER, 'empty');
});
