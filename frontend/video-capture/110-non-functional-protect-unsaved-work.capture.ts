import { expect, test } from '@playwright/test';
import { MentorAttendanceCapturePage, SignInPage, YouthReflectionPage } from '../e2e/pages';
import { seedEngagementCast } from './engagement-cast';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '110-non-functional-protect-unsaved-work';

test.setTimeout(240_000);

test('capture: unsaved reflection draft and unsaved attendance', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.youthEmail, password: cast.youthPassword });
  await page.setViewportSize({ width: 1280, height: 800 });
  const reflectionUrl = `/youth/lessons/${cast.secondLessonId}/reflection`;
  await page.goto(reflectionUrl);

  const reflection = new YouthReflectionPage(page);
  await expect(reflection.heading).toContainText(cast.secondLessonTitle);
  await page.waitForLoadState('networkidle');
  await reflection.reflectionText.fill('I can listen before I try to solve the problem.');
  await page.getByRole('radio', { name: 'Thoughtful' }).check();
  await reflection.optionalResponse.fill('Ask my friend what support would help.');
  await page.evaluate(() => window.scrollTo(0, 560));
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'typed', { x: 380, y: 56, width: 760, height: 744 });

  let prompt = '';
  page.once('dialog', async (dialog) => {
    prompt = dialog.message();
    await dialog.accept();
  });
  await reflection.backToLessonLink.click();
  await expect(page).toHaveURL(new RegExp(`/youth/lessons/${cast.secondLessonId}$`));
  expect(prompt).toMatch(/unsaved reflection changes/i);

  await page.goto(reflectionUrl);
  await expect(reflection.reflectionText).toHaveValue(
    'I can listen before I try to solve the problem.',
  );
  await expect(page.getByRole('status')).toContainText(/draft restored/i);
  await page.waitForLoadState('networkidle');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'restored', { x: 380, y: 80, width: 760, height: 720 });
});

test('capture: unsaved attendance marker', async ({ page, request }) => {
  const cast = await seedEngagementCast(request);
  await signInSlow(page, { email: cast.mentorEmail, password: cast.mentorPassword });
  await page.goto(`/mentor/attendance/${cast.scheduleSessionId}`);
  const capture = new MentorAttendanceCapturePage(page);
  await expect(capture.rows).toHaveCount(1);
  await page.waitForLoadState('networkidle');
  await capture.allPresent.click();
  await capture.note(cast.youthName).fill('Arrived early.');
  await page.mouse.move(0, 0);
  await shot(page, FOLDER, 'attendance', { x: 244, y: 270, width: 1036, height: 330 });
});
