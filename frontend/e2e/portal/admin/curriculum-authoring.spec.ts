import { expect, test } from '@playwright/test';
import {
  BadgeBuilderPage,
  CurriculumBuilderPage,
  ProgressionBuilderPage,
  SignInPage,
} from '../../pages';
import { registerUser, seedYouthCurriculum } from '../../support/api';

// Traces to: L2-CURRIC-001..006, L2-NFR-001
test('an admin authors and publishes the complete curriculum workspace', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'curriculum-builder');
  await new SignInPage(page).signIn(admin);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await expect(builder.heading).toBeVisible();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  await expect(builder.module(seeded.firstModuleName)).toBeVisible();
  await expect(builder.lesson(seeded.firstLessonTitle)).toBeVisible();
  await expect(builder.assignment(seeded.assignmentTitle)).toBeVisible();
  await expect(builder.quiz(seeded.quizId)).toContainText('2 questions');

  const suffix = crypto.randomUUID().slice(0, 8);
  const trackName = 'E2E Track ' + suffix;
  await builder.newTrack.click();
  await builder.trackDialog.getByLabel('Track name').fill(trackName);
  await builder.trackDialog.getByLabel('Track key').fill('e2e-' + suffix);
  await builder.trackDialog.getByLabel('Track description').fill('A browser-authored pathway.');
  await builder.trackDialog.getByRole('button', { name: 'Create track' }).click();
  await expect(builder.trackSelect.locator('option:checked')).toHaveText(trackName);

  const moduleName = 'Foundations ' + suffix;
  await page.getByLabel('Module name').fill(moduleName);
  await page.getByLabel('Module description').fill('Start with service.');
  await page.getByLabel('Scripture anchor').fill('Mark 10:45');
  await page.getByRole('button', { name: 'Add module' }).click();
  const module = builder.module(moduleName);
  await expect(module).toBeVisible();

  const lessonTitle = 'Serve ' + suffix;
  await module.getByRole('button', { name: 'Add lesson' }).click();
  await builder.lessonDialog.getByLabel('Lesson title').fill(lessonTitle);
  await builder.lessonDialog.getByLabel('Teaching body').fill('Serve as Jesus served.');
  await builder.lessonDialog.getByLabel('Leader guide notes').fill('Invite safe examples.');
  await builder.lessonDialog.getByRole('button', { name: 'Create lesson' }).click();
  const lesson = builder.lesson(lessonTitle);
  await expect(lesson).toBeVisible();
  await lesson.getByLabel('Upload PDF or MP4').setInputFiles({
    name: 'leader-guide.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.7\nWord Up curriculum guide'),
  });
  await expect(lesson).toContainText('leader-guide.pdf');
  await lesson.getByRole('button', { name: 'Publish lesson' }).click();

  await module.getByRole('button', { name: 'Add quiz' }).click();
  await builder.quizDialog.getByLabel('Pass mark').fill('80');
  await builder.quizDialog.getByLabel('Attempts allowed').fill('3');
  await builder.quizDialog
    .getByRole('checkbox', { name: 'Required for module completion' })
    .check();
  await builder.quizDialog.getByRole('button', { name: 'Create quiz' }).click();
  await builder.questionEditor.getByLabel('Question text').fill('How will you serve?');
  await builder.questionEditor.getByLabel('Question type').selectOption('ShortAnswer');
  await builder.questionEditor.getByLabel('Correct text answers').fill('with humility');
  await builder.questionEditor.getByRole('button', { name: 'Add question' }).click();
  await expect(builder.questionEditor).toContainText('How will you serve?');

  const assignmentTitle = 'Service reflection ' + suffix;
  await module.getByRole('button', { name: 'Add assignment' }).click();
  await builder.assignmentDialog.getByLabel('Assignment title').fill(assignmentTitle);
  await builder.assignmentDialog
    .getByLabel('Task description')
    .fill('Describe a concrete act of service.');
  await builder.assignmentDialog.getByRole('checkbox', { name: 'Text response' }).check();
  await builder.assignmentDialog.getByLabel('Rubric criterion').fill('Specific and reflective');
  await builder.assignmentDialog.getByLabel('Criterion points').fill('5');
  await builder.assignmentDialog.getByRole('button', { name: 'Create assignment' }).click();
  const assignment = builder.assignment(assignmentTitle);
  await expect(assignment).toContainText('5 points');
  await assignment.getByRole('button', { name: 'Publish assignment' }).click();

  await module.getByRole('button', { name: 'Publish module' }).click();
  await page.getByRole('button', { name: 'Publish track' }).click();
  await expect(builder.outcome).toContainText('published');
});

// Traces to: L2-CURRIC-005
test('an admin creates automatic and manual badges, awards, and archives them', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'badge-builder');
  await new SignInPage(page).signIn(admin);
  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();

  const automaticName = 'Module badge ' + crypto.randomUUID().slice(0, 8);
  await badges.name.fill(automaticName);
  await badges.icon.fill('award');
  await badges.criteriaKind.selectOption('Auto');
  await badges.autoRule.selectOption('module-completed');
  await badges.target.selectOption({ label: seeded.firstModuleName });
  await badges.saveBadge.click();
  const automatic = badges.badge(automaticName);
  await expect(automatic).toContainText('module-completed');
  await automatic.getByRole('button', { name: 'Archive badge' }).click();
  await expect(automatic).toHaveCount(0);

  const manualName = 'Mentor badge ' + crypto.randomUUID().slice(0, 8);
  await badges.name.fill(manualName);
  await badges.icon.fill('star');
  await badges.criteriaKind.selectOption('Manual');
  await badges.saveBadge.click();
  const manual = badges.badge(manualName);
  await manual.getByRole('button', { name: 'Award badge' }).click();
  await badges.youth.selectOption(seeded.youthId);
  await page.getByRole('button', { name: 'Confirm award' }).click();
  await expect(badges.outcome).toContainText(seeded.youthName);
});

// Traces to: L2-CURRIC-006
test('an admin edits ordered stages and removes competencies', async ({ page, request }) => {
  await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'progression-builder');
  await new SignInPage(page).signIn(admin);
  const progression = new ProgressionBuilderPage(page);
  await progression.goto();
  await expect(progression.heading).toBeVisible();

  const suffix = crypto.randomUUID().slice(0, 8);
  const stageName = 'Explorer ' + suffix;
  await progression.addStage.click();
  await progression.stageDialog.getByLabel('Stage name').fill(stageName);
  await progression.stageDialog.getByLabel('Stage description').fill('Beginning the pathway.');
  await progression.stageDialog.getByRole('button', { name: 'Save stage' }).click();
  const stage = progression.stage(stageName);
  await expect(stage).toBeVisible();

  await stage.getByRole('button', { name: 'Add competency' }).click();
  await progression.competencyDialog
    .getByLabel('Competency name')
    .fill('Reads Scripture ' + suffix);
  await progression.competencyDialog.getByRole('button', { name: 'Add competency' }).click();
  await expect(stage).toContainText('Reads Scripture ' + suffix);

  await stage.getByRole('button', { name: 'Edit stage' }).click();
  await progression.stageDialog.getByLabel('Stage name').fill('Explorer ready ' + suffix);
  await progression.stageDialog.getByRole('button', { name: 'Save stage' }).click();
  const updated = progression.stage('Explorer ready ' + suffix);
  await updated.getByRole('button', { name: 'Move stage up' }).click();
  await expect(progression.outcome).toContainText('order saved');
  await updated.getByRole('button', { name: 'Remove competency' }).click();
  await expect(updated).not.toContainText('Reads Scripture ' + suffix);
});

// Traces to: L2-NFR-007
test('an admin saves an owner-bound authoring draft while leaving and restores it', async ({
  page,
  request,
}) => {
  await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'authoring-draft');
  await new SignInPage(page).signIn(admin);
  const badges = new BadgeBuilderPage(page);
  await badges.goto();
  await expect(badges.heading).toBeVisible();

  const draftName = 'Unsaved badge ' + crypto.randomUUID().slice(0, 8);
  await badges.name.fill(draftName);
  await badges.icon.fill('star');
  let prompt = '';
  page.once('dialog', async (dialog) => {
    prompt = dialog.message();
    await dialog.accept();
  });
  await page.getByRole('link', { name: 'Progression', exact: true }).click();

  expect(prompt).toMatch(/save.*authoring.*draft.*cancel/i);
  await expect(page).toHaveURL(/\/admin\/progression$/);
  await page.getByRole('link', { name: 'Badges', exact: true }).click();
  await expect(badges.name).toHaveValue(draftName);
  await expect(badges.icon).toHaveValue('star');
  await expect(page.getByTestId('badge-outcome')).toContainText(/draft restored/i);
});

// Traces to: L2-NFR-007
test('an admin restores the active multi-form curriculum editor after leaving', async ({
  page,
  request,
}) => {
  const seeded = await seedYouthCurriculum(request);
  const admin = await registerUser(request, 'ProgramAdministrator', 'curriculum-draft');
  await new SignInPage(page).signIn(admin);
  const builder = new CurriculumBuilderPage(page);
  await builder.goto();
  await builder.trackSelect.selectOption(seeded.primaryTrackId);
  await expect(builder.module(seeded.firstModuleName)).toBeVisible();

  const draftName = 'Draft module ' + crypto.randomUUID().slice(0, 8);
  await page.getByLabel('Module name').fill(draftName);
  await page.getByLabel('Module description').fill('Restore this unsaved module description.');
  page.once('dialog', async (dialog) => dialog.accept());
  await page.getByRole('link', { name: 'Badges', exact: true }).click();
  await expect(page).toHaveURL(/\/admin\/badges$/);

  await page.getByRole('link', { name: 'Curriculum', exact: true }).click();
  await expect(page.getByLabel('Module name')).toHaveValue(draftName);
  await expect(page.getByLabel('Module description')).toHaveValue(
    'Restore this unsaved module description.',
  );
  await expect(builder.outcome).toContainText(/draft restored/i);
});
