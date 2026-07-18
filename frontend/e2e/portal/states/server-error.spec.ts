import { expect, test } from '@playwright/test';

// Traces to: L2-PLATFORM-008
test('the global 500 handler returns a friendly body with no stack trace', async ({ request }) => {
  const res = await request.get('/api/_diag/throw');
  expect(res.status()).toBe(500);

  const text = await res.text();
  expect(text).toContain('An unexpected error occurred');
  // No internals leaked to the client.
  expect(text).not.toMatch(/StackTrace|InvalidOperationException|WordUp\.|\.cs:\d/);
});
