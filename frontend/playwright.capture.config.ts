import { defineConfig, devices } from '@playwright/test';
import base from './playwright.config';

/**
 * Screen captures for the narrated videos in docs/videos. These are not tests:
 * each file seeds synthetic data, walks one behaviour and saves PNGs next to the
 * video. Run: npx playwright test -c playwright.capture.config.ts <file>
 */
export default defineConfig({
  testDir: './video-capture',
  testMatch: '**/*.capture.ts',
  fullyParallel: false,
  workers: 1,
  reporter: 'list',
  projects: [
    {
      name: 'capture',
      // Reduced motion: stills never catch a dialog or toast mid-animation.
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://localhost:4200',
        reducedMotion: 'reduce',
      },
    },
  ],
  webServer: base.webServer,
});
