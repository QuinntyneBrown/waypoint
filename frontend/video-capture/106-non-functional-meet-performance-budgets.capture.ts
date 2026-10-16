import { expect, test } from '@playwright/test';
import { PrivacySettingsPage } from '../e2e/pages';
import { signInApi } from '../e2e/support/api';
import { signInSlow } from './non-functional-helpers';
import { shot } from './shots';

const FOLDER = '106-non-functional-meet-performance-budgets';

test.setTimeout(300_000);

test('capture: a long history loads one bounded page at a time', async ({ page, request }) => {
  const email = `david-${crypto.randomUUID()}@waypoint.demo`;
  const password = 'Password123';
  const made = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'David Osei', role: 'Youth' },
  });
  expect(made.ok()).toBeTruthy();
  const auth = await signInApi(request, { email, password });
  for (let index = 0; index < 26; index += 1) {
    const response = await request.post('/api/me/data-requests', {
      headers: { Authorization: `Bearer ${auth.accessToken}` },
      data: { actionType: 'access', details: `Request ${index + 1}.` },
    });
    expect(response.ok(), `request ${index + 1} failed`).toBeTruthy();
  }

  await signInSlow(page, { email, password });
  await page.setViewportSize({ width: 1280, height: 800 });
  const privacy = new PrivacySettingsPage(page);
  await privacy.goto();
  await expect(page.getByText(/showing 25 of 26 requests/i)).toBeVisible();
  await page.waitForLoadState('networkidle');
  const showing = page.getByText(/showing 25 of 26 requests/i);
  await showing.scrollIntoViewIfNeeded();
  await page.mouse.move(0, 0);
  const top = (await showing.boundingBox())!;
  await shot(page, FOLDER, 'first-page', {
    x: 244,
    y: Math.max(top.y - 70, 0),
    width: 1036,
    height: 360,
  });

  await privacy.loadMoreButton.scrollIntoViewIfNeeded();
  await page.mouse.move(0, 0);
  const more = (await privacy.loadMoreButton.boundingBox())!;
  await shot(page, FOLDER, 'load-more', {
    x: 244,
    y: Math.max(more.y - 280, 0),
    width: 1036,
    height: 400,
  });

  await privacy.loadMoreButton.click();
  await expect(page.getByText(/showing 26 of 26 requests/i)).toBeVisible();
  await page.getByText(/showing 26 of 26 requests/i).scrollIntoViewIfNeeded();
  await page.mouse.move(0, 0);
  const all = (await page.getByText(/showing 26 of 26 requests/i).boundingBox())!;
  await shot(page, FOLDER, 'all-loaded', {
    x: 244,
    y: Math.max(all.y - 70, 0),
    width: 1036,
    height: 360,
  });
});
