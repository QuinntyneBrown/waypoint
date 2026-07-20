import AxeBuilder from '@axe-core/playwright';
import { expect, Page } from '@playwright/test';

const WCAG_21_AA_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

/** Assert that the page, rather than a deliberately scrollable child, does not overflow. */
export async function expectNoPageOverflow(page: Page): Promise<void> {
  const overflow = await page.evaluate(
    () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
  );
  expect(overflow, 'document-level horizontal overflow in pixels').toBeLessThanOrEqual(1);
}

/** Assert the resolved column count of a responsive CSS grid. */
export async function expectGridColumns(
  page: Page,
  selector: string,
  expectedColumns: number,
): Promise<void> {
  const grid = page.locator(selector);
  await expect(grid).toBeVisible();
  const columns = await grid.evaluate((element) => {
    const template = getComputedStyle(element).gridTemplateColumns.trim();
    return template && template !== 'none' ? template.split(/\s+/).length : 0;
  });
  expect(columns, `${selector} resolved column count`).toBe(expectedColumns);
}

/**
 * Run axe against the rendered page using only WCAG 2.0/2.1 A and AA rules.
 * The compact failure message keeps CI logs actionable without hiding nodes.
 */
export async function expectNoWcag21AaViolations(page: Page): Promise<void> {
  const results = await new AxeBuilder({ page }).withTags(WCAG_21_AA_TAGS).analyze();
  const summary = results.violations
    .map(
      (violation) =>
        `${violation.id} (${violation.impact ?? 'unknown'}): ${violation.help}\n` +
        violation.nodes
          .map((node) => `  ${node.target.join(' ')}: ${node.failureSummary}`)
          .join('\n'),
    )
    .join('\n\n');

  expect(results.violations, summary || 'expected no WCAG 2.1 A/AA violations').toEqual([]);
}
