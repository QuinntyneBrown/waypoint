import { expect, test } from '@playwright/test';
import { AboutPage, ContactPage, DonatePage, LandingPage, ProgramsPage } from '../../e2e/pages';
import { clipOf, openWithDemoContent } from '../public-site-demo';
import { shot } from '../shots';

test.use({ baseURL: 'http://localhost:4201' });

const FOLDER = 'journeys/28-explore-public-site';

test('capture: explore the public site', async ({ page }) => {
  const nav = page.getByRole('navigation', { name: 'Primary' });

  await openWithDemoContent(page, 'home');
  const landing = new LandingPage(page);
  await expect(landing.heroHeading).toBeVisible();
  await expect(landing.impactStat('Cohorts')).toHaveText('2');
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'hero', { x: 0, y: 64, width: 1280, height: 560 });

  await nav.getByRole('link', { name: 'Programs', exact: true }).click();
  await expect(new ProgramsPage(page).track('Bible & Leadership')).toBeVisible();
  await shot(page, FOLDER, 'tracks', await clipOf(page.locator('.ph-grid--2'), 12));

  await nav.getByRole('link', { name: 'About', exact: true }).click();
  await expect(new AboutPage(page).cohort('Regent Park Fall Cohort')).toBeVisible();
  await shot(page, FOLDER, 'cohorts', await clipOf(page.locator('.ph-grid--2'), 12));

  await nav.getByRole('link', { name: 'Get involved', exact: true }).click();
  await expect(page.getByRole('button', { name: /sign up to volunteer/i })).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'involved', { x: 0, y: 70, width: 1280, height: 560 });

  await page.getByRole('link', { name: 'Donate', exact: true }).first().click();
  const donate = new DonatePage(page);
  await expect(donate.continueButton).toBeVisible();
  await page.waitForLoadState('networkidle');
  await donate.donorName.fill('Sam Whitfield');
  await donate.email.fill('sam.whitfield@example.com');
  await donate.tier('$50/mo').click();
  await shot(page, FOLDER, 'donate', { x: 300, y: 70, width: 680, height: 590 });

  await nav.getByRole('link', { name: 'Contact', exact: true }).click();
  const contact = new ContactPage(page);
  await expect(contact.sendButton).toBeVisible();
  await page.evaluate(() => window.scrollTo(0, 0));
  await shot(page, FOLDER, 'contact', { x: 300, y: 70, width: 680, height: 430 });
});
