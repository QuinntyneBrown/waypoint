import { expect, test } from '@playwright/test';
import { MentorFlagsPage, SignInPage } from '../e2e/pages';
import { seedCast } from './mentoring-cast';
import { shot } from './shots';
import { shotOf } from './shotOf';

const FOLDER = '055-mentoring-manage-at-risk-flags';
const MAIN = { x: 250, y: 70, width: 1030, height: 730 };

test('capture: manage at-risk flags', async ({ page, request }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  const seeded = await seedCast(request, 'David Osei');
  await new SignInPage(page).signIn({ email: seeded.mentorEmail, password: seeded.mentorPassword });
  await page.goto('/mentor/flags');

  const atRisk = new MentorFlagsPage(page);
  const flag = atRisk.flag('David Osei');
  await expect(flag).toContainText('Missed 1 session');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'list', MAIN);

  await flag.getByRole('button', { name: /^message$/i }).click();
  await page.getByLabel('Message David Osei').fill('Checking in after your missed session.');
  await shot(page, FOLDER, 'message', MAIN);
  await page.getByRole('button', { name: /send message/i }).click();
  await expect(atRisk.status).toContainText('Message sent to David Osei');

  await flag.getByRole('button', { name: /unflag/i }).click();
  await page.getByLabel(/resolution reason/i).selectOption('FlaggedInError');
  await shotOf(page, FOLDER, 'unflag', flag, 8, 1030, 760);
  await page.getByRole('button', { name: /confirm unflag/i }).click();
  await expect(atRisk.status).toContainText('Flag resolved for David Osei');
  await expect(page.getByRole('heading', { name: /no active flags/i })).toBeVisible();
  await shot(page, FOLDER, 'resolved', MAIN);

  await atRisk.youth.selectOption(seeded.youthId);
  await atRisk.manualReason.fill('Safeguarding follow-up after a conversation at the session.');
  await atRisk.raise.click();
  await expect(atRisk.status).toContainText('Flag raised for David Osei');
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'raised', MAIN);
});
