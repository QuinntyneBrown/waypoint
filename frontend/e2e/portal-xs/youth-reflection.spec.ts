import { expect, test } from '@playwright/test';
import { SignInPage, YouthReflectionPage } from '../pages';
import { seedYouthCurriculum } from '../support/api';

// Traces to: L2-LEARN-003, L2-NFR-007, L2-NFR-009, L2-NFR-010
test('the reflection editor stays contained and keyboard operable on a mobile viewport', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  await page.goto(`/youth/lessons/${seeded.secondLessonId}/reflection`);

  const reflection = new YouthReflectionPage(page);
  await expect(reflection.heading).toContainText(seeded.secondLessonTitle);
  expect(await page.evaluate(() => document.documentElement.scrollWidth - innerWidth)).toBeLessThanOrEqual(
    1,
  );
  await expect(reflection.shareWithMentor).not.toBeChecked();

  for (const target of [reflection.saveDraftButton, reflection.submitButton]) {
    const box = await target.boundingBox();
    expect(box?.height ?? 0).toBeGreaterThanOrEqual(44);
  }
  const hopefulLabel = page.locator('label').filter({ hasText: 'Hopeful' });
  expect((await hopefulLabel.boundingBox())?.height ?? 0).toBeGreaterThanOrEqual(44);

  await reflection.reflectionText.focus();
  await page.keyboard.type('I will make room to listen well.');
  const hopeful = page.getByRole('radio', { name: 'Hopeful' });
  await hopeful.focus();
  await page.keyboard.press('Space');
  await reflection.submitButton.focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('status')).toContainText(/reflection submitted/i);
});
