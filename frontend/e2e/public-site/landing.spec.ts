import { expect, test } from '@playwright/test';
import { ApplyPage, LandingPage } from '../pages';

// Traces to: L2-PUB-001
test('the landing page renders for anonymous visitors with journey, impact and CTAs', async ({
  page,
}) => {
  const landing = new LandingPage(page);
  const res = await landing.goto();
  expect(res?.status()).toBe(200);
  await expect(page).not.toHaveURL(/sign-in/);

  await expect(landing.heroHeading).toBeVisible();

  // The 4-step journey (Apply → Join a cohort → Learn the modules → Lead & graduate).
  for (const step of ['Apply', 'Join a cohort', 'Learn the modules', 'Lead & graduate']) {
    await expect(landing.journeyStep(step)).toBeVisible();
  }

  // Primary CTAs are present (Apply, Sign in, Get involved, Contact).
  await expect(landing.signInLink).toBeVisible();
  await expect(landing.getInvolvedLink).toBeVisible();
  await expect(landing.contactLink).toBeVisible();

  // The Apply CTA navigates.
  await landing.applyCta.click();
  await expect(page).toHaveURL(new RegExp(ApplyPage.path));
});

// Traces to: L2-PUB-001
test('Sign in crosses from the public app to the portal app', async ({ page }) => {
  const landing = new LandingPage(page);
  await landing.goto();

  await landing.signInLink.click();

  await expect(page).toHaveURL('http://localhost:4200/auth/sign-in');
  await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
});
