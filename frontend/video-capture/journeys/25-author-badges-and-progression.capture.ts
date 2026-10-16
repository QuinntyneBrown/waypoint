import { expect, test } from '@playwright/test';
import { BadgeBuilderPage, ProgressionBuilderPage } from '../../e2e/pages';
import { seedYouthCurriculum } from '../../e2e/support/api';
import { shotAround, shotOf, signInAsPat } from '../curriculum-authoring.helpers';

const FOLDER = 'journeys/25-author-badges-and-progression';

test('capture: author badges and progression', async ({ page, request }) => {
  const seeded = await seedYouthCurriculum(request);
  const amara = await request.post('/api/_test/users', {
    data: {
      email: `amara-${crypto.randomUUID()}@waypoint.demo`,
      password: 'Password123',
      displayName: 'Amara Bell',
      role: 'Youth',
    },
  });
  expect(amara.ok()).toBeTruthy();
  const { userId } = (await amara.json()) as { userId: string };
  await signInAsPat(page, request);

  const progression = new ProgressionBuilderPage(page);
  await progression.goto();
  await expect(progression.heading).toBeVisible();
  const tag = crypto.randomUUID().slice(0, 6);
  const first = 'Explorer ' + tag;
  const second = 'Servant leader ' + tag;
  await progression.addStage.click();
  await progression.stageDialog.getByLabel('Stage name').fill(first);
  await progression.stageDialog
    .getByLabel('Stage description')
    .fill('Beginning the pathway and learning the habits.');
  await progression.stageDialog.getByRole('button', { name: 'Save stage' }).click();
  const stageOne = progression.stage(first);
  await expect(stageOne).toBeVisible();
  await progression.addStage.click();
  await progression.stageDialog.getByLabel('Stage name').fill(second);
  await progression.stageDialog
    .getByLabel('Stage description')
    .fill('Taking responsibility and serving others.');
  await progression.stageDialog.getByRole('button', { name: 'Save stage' }).click();
  const stageTwo = progression.stage(second);
  await expect(stageTwo).toBeVisible();
  for (const [stage, name] of [
    [stageOne, 'Reads Scripture daily'],
    [stageOne, 'Prays with a partner'],
    [stageTwo, 'Leads a small group'],
  ] as const) {
    await stage.getByRole('button', { name: 'Add competency' }).click();
    await progression.competencyDialog.getByLabel('Competency name').fill(name);
    await progression.competencyDialog.getByRole('button', { name: 'Add competency' }).click();
    await expect(stage).toContainText(name);
  }
  // Stage numbers depend on other stages in the shared demo data, so leave them out.
  await stageOne.locator('.w-eyebrow').evaluate((el) => (el.style.visibility = 'hidden'));
  await shotAround(
    [
      stageOne.getByRole('button', { name: 'Move stage up' }),
      stageOne.locator('h2'),
      stageOne.getByRole('button', { name: 'Add competency' }),
      stageOne.locator('ul.competency-list'),
    ],
    FOLDER,
    'stage',
    8,
  );
  await stageTwo.getByRole('button', { name: 'Move stage up' }).click();
  await expect(progression.outcome).toContainText('Framework order saved.');
  await shotOf(progression.outcome, FOLDER, 'order-saved', 12);

  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();
  const form = page.locator('form.admin-form');
  const autoName = 'Foundations finisher ' + tag;
  await badges.name.fill(autoName);
  await badges.icon.fill('award');
  await page.getByLabel('Badge description').fill('Earned by finishing a whole module.');
  await badges.criteriaKind.selectOption('Auto');
  await badges.autoRule.selectOption('module-completed');
  await badges.target.selectOption({ label: seeded.firstModuleName });
  await badges.competency.selectOption({ label: seeded.mentorCompetencyName });
  await shotOf(form, FOLDER, 'auto-form', 16);
  await badges.saveBadge.click();
  const automatic = badges.badge(autoName);
  await expect(automatic).toContainText('module-completed');
  await shotOf(automatic, FOLDER, 'auto-card', 12);

  const manualName = 'Servant heart ' + tag;
  await badges.name.fill(manualName);
  await badges.icon.fill('star');
  await page.getByLabel('Badge description').fill('Recognition a mentor gives by hand.');
  await badges.criteriaKind.selectOption('Manual');
  await badges.saveBadge.click();
  const manual = badges.badge(manualName);
  await expect(manual.getByRole('button', { name: 'Award badge' })).toBeVisible();
  await manual.getByRole('button', { name: 'Award badge' }).click();
  const award = page.getByRole('dialog', { name: 'Award ' + manualName });
  await expect(award).toBeVisible();
  await badges.youth.selectOption(userId);
  await shotOf(award, FOLDER, 'award-dialog');
  await page.getByRole('button', { name: 'Confirm award' }).click();
  await expect(badges.outcome).toContainText('awarded to Amara Bell');
  await expect(manual).toContainText('1');
  await shotOf(manual, FOLDER, 'manual-card', 12);
});
