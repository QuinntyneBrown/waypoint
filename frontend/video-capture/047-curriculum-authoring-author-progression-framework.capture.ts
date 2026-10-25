import { expect, test } from '@playwright/test';
import { ProgressionBuilderPage } from '../e2e/pages';
import { shotAround, shotOf, signInAsPat } from './curriculum-authoring.helpers';

const FOLDER = '047-curriculum-authoring-author-progression-framework';

test('capture: author the progression framework', async ({ page, request }) => {
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
  await shotOf(progression.stageDialog, FOLDER, 'stage-dialog');
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

  const addCompetency = async (stage: ReturnType<typeof progression.stage>, name: string) => {
    await stage.getByRole('button', { name: 'Add competency' }).click();
    await progression.competencyDialog.getByLabel('Competency name').fill(name);
    if (name === 'Reads Scripture daily') {
      await shotOf(progression.competencyDialog, FOLDER, 'competency-dialog');
    }
    await progression.competencyDialog.getByRole('button', { name: 'Add competency' }).click();
    await expect(stage).toContainText(name);
  };
  await addCompetency(stageOne, 'Reads Scripture daily');
  await addCompetency(stageOne, 'Prays with a partner');
  await addCompetency(stageTwo, 'Leads a small group');
  // Stage numbers depend on other stages in the shared demo data, so leave them out.
  await stageOne.locator('.ph-eyebrow').evaluate((el) => (el.style.visibility = 'hidden'));
  await shotAround(
    [
      stageOne.getByRole('button', { name: 'Move stage up' }),
      stageOne.locator('h2'),
      stageOne.getByRole('button', { name: 'Add competency' }),
      stageOne.locator('ul.competency-list'),
    ],
    FOLDER,
    'stage-one',
    8,
  );

  await stageTwo.getByRole('button', { name: 'Move stage up' }).click();
  await expect(progression.outcome).toContainText('Framework order saved.');
  await shotOf(progression.outcome, FOLDER, 'order-saved', 12);

  await progression
    .stage(first)
    .locator('li', { hasText: 'Prays with a partner' })
    .getByRole('button', { name: 'Remove competency' })
    .click();
  await expect(progression.outcome).toContainText('removed from the active framework');
  await shotOf(progression.outcome, FOLDER, 'removed', 12);
});
