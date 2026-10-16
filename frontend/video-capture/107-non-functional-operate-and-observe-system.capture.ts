import { expect, test } from '@playwright/test';
import { shot } from './shots';

const FOLDER = '107-non-functional-operate-and-observe-system';
const API = 'http://127.0.0.1:5211';

test('capture: health probes and the maintenance page', async ({ page }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto(`${API}/health`);
  await expect(page.locator('body')).toHaveText('Healthy');
  await shot(page, FOLDER, 'live', { x: 0, y: 0, width: 360, height: 180 });

  await page.goto(`${API}/health/ready`);
  await expect(page.locator('body')).toHaveText('Healthy');
  await shot(page, FOLDER, 'ready', { x: 0, y: 0, width: 360, height: 180 });

  await page.goto('http://localhost:4200/maintenance');
  await expect(page.getByRole('heading', { name: /we.ll be right back/i })).toBeVisible();
  await page.waitForLoadState('networkidle');
  await shot(page, FOLDER, 'maintenance', { x: 0, y: 0, width: 1280, height: 640 });
});
