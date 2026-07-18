import { expect, test } from '@playwright/test';
import { SignInPage, YouthReflectionPage } from '../../pages';
import { seedYouthCurriculum } from '../../support/api';

// Traces to: L2-LEARN-003, L2-NFR-007
test('youth saves and restores a private draft before submitting a mentor-shared reflection', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const reflectionUrl = `/youth/lessons/${seeded.secondLessonId}/reflection`;
  await page.goto(reflectionUrl);

  const reflection = new YouthReflectionPage(page);
  await expect(reflection.heading).toContainText(seeded.secondLessonTitle);
  await expect(page.getByText(/practise neighbour-love/i)).toBeVisible();
  await expect(reflection.shareWithMentor).not.toBeChecked();

  await reflection.submitButton.click();
  await expect(page.getByText(/reflection text is required/i)).toBeVisible();
  await expect(page.getByText(/choose how you are feeling/i)).toBeVisible();

  await reflection.reflectionText.fill('I can listen before I try to solve the problem.');
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await reflection.optionalResponse.fill('Ask my friend what support would help.');

  page.once('dialog', async (dialog) => {
    expect(dialog.message()).toMatch(/save.*draft.*cancel/i);
    await dialog.dismiss();
  });
  await reflection.backToLessonLink.click();
  await expect(page).toHaveURL(new RegExp(`${reflectionUrl}$`));

  page.once('dialog', async (dialog) => dialog.accept());
  await reflection.backToLessonLink.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${seeded.secondLessonId}$`));

  await page.goto(reflectionUrl);
  await expect(reflection.reflectionText).toHaveValue(
    'I can listen before I try to solve the problem.',
  );
  await expect(page.getByRole('radio', { name: 'Thoughtful' })).toBeChecked();
  await expect(reflection.optionalResponse).toHaveValue('Ask my friend what support would help.');
  await expect(reflection.shareWithMentor).not.toBeChecked();
  await expect(page.getByRole('status')).toContainText(/draft restored/i);

  await reflection.shareWithMentor.check();
  await reflection.submitButton.click();
  await expect(page.getByRole('status')).toContainText(/reflection submitted/i);
});
