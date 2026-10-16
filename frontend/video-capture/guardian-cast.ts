import { APIRequestContext, Locator, Page, expect } from '@playwright/test';
import { SeededGuardianFamily, seedGuardianFamily } from '../e2e/support/api';
import { rename } from './mentoring-cast';
import { shotFull } from './shotOf';
import { shot } from './shots';

export interface GuardianCast {
  guardian: { email: string; password: string; userId: string; displayName: string };
  family: SeededGuardianFamily;
}

/** Seed Ruth Clarke with a linked child (renamed to a cast name) through the Testing endpoints. */
export async function seedGuardianCast(
  request: APIRequestContext,
  childName: string,
  includeConsent = true,
): Promise<GuardianCast> {
  const email = `ruth-${crypto.randomUUID().slice(0, 8)}@waypoint.demo`;
  const password = 'Password123';
  const res = await request.post('/api/_test/users', {
    data: { email, password, displayName: 'Ruth Clarke', role: 'Guardian' },
  });
  expect(res.ok(), `seed guardian failed: ${res.status()}`).toBeTruthy();
  const { userId } = (await res.json()) as { userId: string };
  const family = await seedGuardianFamily(request, userId, { includeConsent });
  await rename(request, { email: family.childEmail, password: family.childPassword }, childName);
  return {
    guardian: { email, password, userId, displayName: 'Ruth Clarke' },
    family: { ...family, childName },
  };
}

/** Crop a full-page screenshot to an element (document coordinates), for regions taller than the viewport. */
export async function shotElement(
  page: Page,
  folder: string,
  name: string,
  target: Locator,
  pad = 12,
): Promise<void> {
  const box = (await target.boundingBox())!;
  const scrollY = await page.evaluate(() => window.scrollY);
  await shotFull(page, folder, name, {
    x: Math.max(0, box.x - pad),
    y: Math.max(0, box.y + scrollY - pad),
    width: box.width + pad * 2,
    height: box.height + pad * 2,
  });
}

/** Crop the viewport above a status message so the message is shown with the card it belongs to. */
export async function shotWithMessage(
  page: Page,
  folder: string,
  name: string,
  message: Locator,
): Promise<void> {
  await message.scrollIntoViewIfNeeded();
  const box = (await message.boundingBox())!;
  const y = Math.max(0, box.y - 250);
  await shot(page, folder, name, {
    x: 244,
    y,
    width: 1036,
    height: Math.min(800, box.y + box.height + 16) - y,
  });
}

/** Let hover and transition effects finish so the screenshot shows the resting styles. */
export async function settle(page: Page): Promise<void> {
  await page.mouse.move(2, 2);
  await page.evaluate(
    () => document.activeElement instanceof HTMLElement && document.activeElement.blur(),
  );
  await page.evaluate(() => Promise.all(document.getAnimations().map((a) => a.finished)));
  await page.waitForTimeout(500);
}
