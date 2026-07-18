import { expect, test } from '@playwright/test';
import { SignInPage, YouthCodingPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-008, L2-NFR-009, L2-NFR-010
test('coding editor, preview, steps, and checklist remain contained and keyboard operable on mobile', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/coding/${seeded.codingLessonId}`);

  const coding = new YouthCodingPage(page);
  await expect(coding.heading).toHaveText(seeded.codingLessonTitle);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
  expect((await coding.editor.boundingBox())?.width ?? Infinity).toBeLessThanOrEqual(412);
  expect((await coding.preview.boundingBox())?.width ?? Infinity).toBeLessThanOrEqual(412);

  await coding.editor.focus();
  await page.keyboard.press('Control+A');
  await page.keyboard.type('<h1>Mobile coding works</h1>');
  await page.getByRole('checkbox', { name: /welcome heading/i }).focus();
  await page.keyboard.press('Space');
  await coding.step(seeded.codingSecondStepTitle).focus();
  await page.keyboard.press('Enter');
  await page.getByRole('checkbox', { name: /introduction paragraph/i }).focus();
  await page.keyboard.press('Space');
  await expect(coding.progress).toHaveText('100%');
  expect((await coding.submitButton.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);
  expect(
    await page.evaluate(() => document.documentElement.scrollWidth - innerWidth),
  ).toBeLessThanOrEqual(1);
});
