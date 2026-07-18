import { expect, test } from '@playwright/test';
import { MentorFlagsPage, SignInPage } from '../../pages';
import { seedYouthCurriculum, signInApi } from '../../support/api';

// Traces to: L2-MENTOR-007, L2-ENGAGE-007, L2-AUTH-010
test('mentor acts on an at-risk signal and records a confirmed resolution', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  await new SignInPage(page).signIn({
    email: seeded.mentorEmail,
    password: seeded.mentorPassword,
  });
  await page.goto('/mentor/flags');

  const atRisk = new MentorFlagsPage(page);
  await expect(atRisk.heading).toBeVisible();
  const flag = atRisk.flag(seeded.youthName);
  await expect(flag).toContainText('Follow up after two missed sessions.');
  await expect(flag).toContainText('Missed 1 session');
  await expect(flag.getByText(/flagged by/i)).toContainText(seeded.mentorName);

  const checkIn = flag.getByRole('link', { name: /log check-in/i });
  await expect(checkIn).toHaveAttribute('href', new RegExp(`youthId=${seeded.youthId}`));

  const message = `Checking in after your missed session ${crypto.randomUUID()}.`;
  await flag.getByRole('button', { name: /^message$/i }).click();
  await page.getByLabel(`Message ${seeded.youthName}`).fill(message);
  await page.getByRole('button', { name: /send message/i }).click();
  await expect(atRisk.status).toContainText(`Message sent to ${seeded.youthName}`);

  const youth = await signInApi(request, {
    email: seeded.youthEmail,
    password: seeded.youthPassword,
  });
  const youthThread = await request.get('/api/me/messages', {
    headers: { Authorization: `Bearer ${youth.accessToken}` },
  });
  expect(youthThread.ok(), await youthThread.text()).toBeTruthy();
  expect(await youthThread.text()).toContain(message);

  await flag.getByRole('button', { name: /unflag/i }).click();
  await page.getByLabel(/resolution reason/i).selectOption('FlaggedInError');
  await page.getByRole('button', { name: /confirm unflag/i }).click();
  await expect(atRisk.status).toContainText(`Flag resolved for ${seeded.youthName}`);
  await expect(page.getByRole('heading', { name: /no active flags/i })).toBeVisible();

  const manualReason = `Safeguarding follow-up ${crypto.randomUUID()}.`;
  await atRisk.youth.selectOption(seeded.youthId);
  await atRisk.manualReason.fill(manualReason);
  await atRisk.raise.click();
  await expect(atRisk.status).toContainText(`Flag raised for ${seeded.youthName}`);
  await expect(atRisk.flag(seeded.youthName)).toContainText(manualReason);
});
