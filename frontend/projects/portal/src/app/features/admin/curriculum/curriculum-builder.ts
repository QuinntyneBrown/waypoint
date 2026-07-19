import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  AssignmentWorkspace,
  CurriculumAuthoringApi,
  LessonWorkspace,
  ModuleWorkspace,
  QuizDetail,
  QuizWorkspace,
  SaveAssignment,
  SaveLesson,
  SaveModule,
  SaveQuestion,
  SaveQuiz,
  TrackSummary,
  TrackWorkspace,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { finalize, Observable } from 'rxjs';

// Traces to: L2-CURRIC-001..004, L2-NFR-001, L2-NFR-009, L2-NFR-010
@Component({
  selector: 'wu-curriculum-builder',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, ReactiveFormsModule],
  templateUrl: './curriculum-builder.html',
  styleUrl: './curriculum-builder.css',
})
export class CurriculumBuilderPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(CurriculumAuthoringApi);
  private readonly fb = inject(FormBuilder);

  protected readonly tracks = signal<TrackSummary[]>([]);
  protected readonly workspace = signal<TrackWorkspace | null>(null);
  protected readonly selectedTrackId = signal('');
  protected readonly selectedQuiz = signal<QuizDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly trackDialogOpen = signal(false);
  protected readonly lessonModuleId = signal<string | null>(null);
  protected readonly quizModuleId = signal<string | null>(null);
  protected readonly assignmentModuleId = signal<string | null>(null);
  protected readonly editingModuleId = signal<string | null>(null);
  protected readonly editingLessonId = signal<string | null>(null);
  protected readonly editingAssignmentId = signal<string | null>(null);

  protected readonly trackForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    key: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', Validators.maxLength(2000)],
  });
  protected readonly moduleForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    description: ['', Validators.maxLength(2000)],
    scriptureAnchor: ['', Validators.maxLength(300)],
    prerequisiteModuleId: [''],
  });
  protected readonly lessonForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    scriptureReference: ['', Validators.maxLength(300)],
    estimatedMinutes: [null as number | null],
    learningObjectives: ['', Validators.maxLength(4000)],
    teachingBody: ['', Validators.maxLength(20000)],
    reflectionPrompts: ['', Validators.maxLength(4000)],
    leaderGuideNotes: ['', Validators.maxLength(10000)],
  });
  protected readonly quizForm = this.fb.nonNullable.group({
    passMark: [80, [Validators.required, Validators.min(0), Validators.max(100)]],
    attemptsAllowed: [3, [Validators.required, Validators.min(1)]],
    feedbackMode: ['show-correct', Validators.required],
    shuffle: false,
    requiredToCompleteModule: false,
  });
  protected readonly questionForm = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(2000)]],
    type: ['MultipleChoice' as SaveQuestion['type'], Validators.required],
    options: [''],
    correctIndexes: [''],
    correctAnswers: [''],
  });
  protected readonly assignmentForm = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    taskDescription: ['', [Validators.required, Validators.maxLength(10000)]],
    textResponse: false,
    photoUpload: false,
    link: false,
    fileUpload: false,
    dueDate: [''],
    gracePeriodDays: [0, [Validators.required, Validators.min(0)]],
    allowLate: true,
    requireMentorApproval: true,
    rubric: this.fb.array([this.rubricRow()]),
  });

  constructor() {
    this.loadTracks();
  }

  protected get rubric(): FormArray {
    return this.assignmentForm.controls.rubric;
  }

  protected selectTrack(id: string): void {
    if (!id) return;
    this.selectedTrackId.set(id);
    this.selectedQuiz.set(null);
    this.loadWorkspace(id);
  }

  protected openTrackDialog(): void {
    this.trackForm.reset({ name: '', key: '', description: '' });
    this.trackDialogOpen.set(true);
  }

  protected createTrack(): void {
    if (this.trackForm.invalid || this.busy()) {
      this.trackForm.markAllAsTouched();
      return;
    }
    const value = this.trackForm.getRawValue();
    this.start();
    this.api
      .createTrack({
        name: value.name.trim(),
        key: value.key.trim(),
        description: this.clean(value.description),
      })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (result) => {
          this.trackDialogOpen.set(false);
          this.outcome.set('Track created.');
          this.loadTracks(result.id);
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected saveModule(): void {
    const trackId = this.selectedTrackId();
    if (!trackId || this.moduleForm.invalid || this.busy()) {
      this.moduleForm.markAllAsTouched();
      return;
    }
    const value = this.moduleForm.getRawValue();
    const request: SaveModule = {
      trackId,
      name: value.name.trim(),
      description: this.clean(value.description),
      scriptureAnchor: this.clean(value.scriptureAnchor),
      prerequisiteModuleId: value.prerequisiteModuleId || null,
    };
    const editingId = this.editingModuleId();
    const operation = editingId
      ? this.api.updateModule(editingId, request)
      : this.api.createModule(request);
    this.run(operation, editingId ? 'Module updated.' : 'Module added.', () => {
      this.cancelModuleEdit();
      this.loadWorkspace(trackId);
    });
  }

  protected editModule(module: ModuleWorkspace): void {
    this.editingModuleId.set(module.id);
    this.moduleForm.reset({
      name: module.name,
      description: module.description ?? '',
      scriptureAnchor: module.scriptureAnchor ?? '',
      prerequisiteModuleId: module.prerequisiteModuleId ?? '',
    });
  }

  protected cancelModuleEdit(): void {
    this.editingModuleId.set(null);
    this.moduleForm.reset({
      name: '',
      description: '',
      scriptureAnchor: '',
      prerequisiteModuleId: '',
    });
  }

  protected moveModule(moduleId: string, direction: -1 | 1): void {
    const workspace = this.workspace();
    if (!workspace) return;
    const ids = workspace.modules.map((item) => item.id);
    const current = ids.indexOf(moduleId);
    const target = current + direction;
    if (current < 0 || target < 0 || target >= ids.length) return;
    [ids[current], ids[target]] = [ids[target], ids[current]];
    this.run(this.api.reorderModules(workspace.id, ids), 'Module order saved.', () =>
      this.loadWorkspace(workspace.id),
    );
  }

  protected publishTrack(): void {
    const workspace = this.workspace();
    if (!workspace) return;
    this.run(this.api.publishTrack(workspace.id, 'Published'), 'Track published.', () =>
      this.loadWorkspace(workspace.id),
    );
  }

  protected publishModule(module: ModuleWorkspace): void {
    this.run(this.api.publishModule(module.id, 'Published'), 'Module published.', () =>
      this.reload(),
    );
  }

  protected openLesson(moduleId: string, lesson?: LessonWorkspace): void {
    this.lessonModuleId.set(moduleId);
    this.editingLessonId.set(lesson?.id ?? null);
    this.lessonForm.reset({
      title: lesson?.title ?? '',
      scriptureReference: lesson?.scriptureReference ?? '',
      estimatedMinutes: lesson?.estimatedMinutes ?? null,
      learningObjectives: lesson?.learningObjectives ?? '',
      teachingBody: lesson?.teachingBody ?? '',
      reflectionPrompts: lesson?.reflectionPrompts ?? '',
      leaderGuideNotes: lesson?.leaderGuideNotes ?? '',
    });
  }

  protected closeLesson(): void {
    this.lessonModuleId.set(null);
    this.editingLessonId.set(null);
  }

  protected saveLesson(): void {
    const moduleId = this.lessonModuleId();
    if (!moduleId || this.lessonForm.invalid || this.busy()) {
      this.lessonForm.markAllAsTouched();
      return;
    }
    const value = this.lessonForm.getRawValue();
    const request: SaveLesson = {
      moduleId,
      title: value.title.trim(),
      scriptureReference: this.clean(value.scriptureReference),
      estimatedMinutes: value.estimatedMinutes,
      learningObjectives: this.clean(value.learningObjectives),
      teachingBody: this.clean(value.teachingBody),
      reflectionPrompts: this.clean(value.reflectionPrompts),
      leaderGuideNotes: this.clean(value.leaderGuideNotes),
    };
    const editingId = this.editingLessonId();
    const operation = editingId
      ? this.api.updateLesson(editingId, request)
      : this.api.createLesson(request);
    this.run(operation, editingId ? 'Lesson updated.' : 'Lesson created.', () => {
      this.closeLesson();
      this.reload();
    });
  }

  protected publishLesson(lesson: LessonWorkspace): void {
    this.run(this.api.publishLesson(lesson.id, 'Published'), 'Lesson published.', () =>
      this.reload(),
    );
  }

  protected archiveLesson(lesson: LessonWorkspace): void {
    this.run(this.api.archiveLesson(lesson.id), 'Lesson archived.', () => this.reload());
  }

  protected moveLesson(module: ModuleWorkspace, lessonId: string, direction: -1 | 1): void {
    const ids = module.lessons.map((item) => item.id);
    const current = ids.indexOf(lessonId);
    const target = current + direction;
    if (current < 0 || target < 0 || target >= ids.length) return;
    [ids[current], ids[target]] = [ids[target], ids[current]];
    this.run(this.api.reorderLessons(module.id, ids), 'Lesson order saved.', () => this.reload());
  }

  protected uploadMedia(lesson: LessonWorkspace, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    this.run(this.api.uploadLessonMedia(lesson.id, file), 'Lesson media uploaded.', () => {
      input.value = '';
      this.reload();
    });
  }

  protected removeMedia(lessonId: string, mediaId: string): void {
    this.run(this.api.removeLessonMedia(lessonId, mediaId), 'Lesson media removed.', () =>
      this.reload(),
    );
  }

  protected openQuizDialog(moduleId: string): void {
    this.quizModuleId.set(moduleId);
    this.quizForm.reset({
      passMark: 80,
      attemptsAllowed: 3,
      feedbackMode: 'show-correct',
      shuffle: false,
      requiredToCompleteModule: false,
    });
  }

  protected closeQuizDialog(): void {
    this.quizModuleId.set(null);
  }

  protected createQuiz(): void {
    const moduleId = this.quizModuleId();
    if (!moduleId || this.quizForm.invalid || this.busy()) {
      this.quizForm.markAllAsTouched();
      return;
    }
    const request = this.quizRequest(moduleId, null);
    this.start();
    this.api
      .createQuiz(request)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (result) => {
          this.closeQuizDialog();
          this.outcome.set('Quiz created. Add its questions below.');
          this.loadWorkspace(this.selectedTrackId());
          this.openQuiz(result.id);
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected openQuiz(id: string): void {
    this.errorMessage.set(null);
    this.api.quiz(id).subscribe({
      next: (quiz) => {
        this.selectedQuiz.set(quiz);
        this.quizForm.reset({
          passMark: quiz.passMark,
          attemptsAllowed: quiz.attemptsAllowed,
          feedbackMode: quiz.feedbackMode,
          shuffle: quiz.shuffle,
          requiredToCompleteModule: quiz.requiredToCompleteModule,
        });
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected updateQuiz(): void {
    const quiz = this.selectedQuiz();
    if (!quiz || this.quizForm.invalid) return;
    this.run(
      this.api.updateQuiz(quiz.id, this.quizRequest(quiz.moduleId, quiz.lessonId)),
      'Quiz settings saved.',
      () => {
        this.openQuiz(quiz.id);
        this.reload();
      },
    );
  }

  protected addQuestion(): void {
    const quiz = this.selectedQuiz();
    if (!quiz || this.questionForm.invalid || this.busy()) {
      this.questionForm.markAllAsTouched();
      return;
    }
    const value = this.questionForm.getRawValue();
    const request: SaveQuestion = {
      text: value.text.trim(),
      type: value.type,
      options: value.type === 'TrueFalse' ? ['True', 'False'] : this.lines(value.options),
      correctIndexes: value.correctIndexes
        .split(/[,\s]+/)
        .map((part) => Number(part) - 1)
        .filter((index) => Number.isInteger(index) && index >= 0),
      correctAnswers: this.lines(value.correctAnswers),
    };
    this.run(this.api.addQuestion(quiz.id, request), 'Question added.', () => {
      this.questionForm.reset({
        text: '',
        type: 'MultipleChoice',
        options: '',
        correctIndexes: '',
        correctAnswers: '',
      });
      this.openQuiz(quiz.id);
      this.reload();
    });
  }

  protected moveQuestion(questionId: string, direction: -1 | 1): void {
    const quiz = this.selectedQuiz();
    if (!quiz) return;
    const ids = quiz.questions.map((item) => item.id);
    const current = ids.indexOf(questionId);
    const target = current + direction;
    if (current < 0 || target < 0 || target >= ids.length) return;
    [ids[current], ids[target]] = [ids[target], ids[current]];
    this.run(this.api.reorderQuestions(quiz.id, ids), 'Question order saved.', () =>
      this.openQuiz(quiz.id),
    );
  }

  protected openAssignment(moduleId: string, assignment?: AssignmentWorkspace): void {
    this.assignmentModuleId.set(moduleId);
    this.editingAssignmentId.set(assignment?.id ?? null);
    if (!assignment) {
      this.resetAssignmentForm();
      return;
    }
    this.api.assignment(assignment.id).subscribe({
      next: (detail) => {
        this.rubric.clear();
        for (const criterion of detail.rubric) {
          this.rubric.push(this.rubricRow(criterion.description, criterion.points));
        }
        if (this.rubric.length === 0) this.rubric.push(this.rubricRow());
        this.assignmentForm.patchValue({
          title: detail.title,
          taskDescription: detail.taskDescription,
          textResponse: detail.submissionTypes.includes('text'),
          photoUpload: detail.submissionTypes.includes('photo'),
          link: detail.submissionTypes.includes('link'),
          fileUpload: detail.submissionTypes.includes('file'),
          dueDate: detail.dueDate?.slice(0, 10) ?? '',
          gracePeriodDays: detail.gracePeriodDays,
          allowLate: detail.allowLate,
          requireMentorApproval: detail.requireMentorApproval,
        });
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected closeAssignment(): void {
    this.assignmentModuleId.set(null);
    this.editingAssignmentId.set(null);
  }

  protected addRubricRow(): void {
    this.rubric.push(this.rubricRow());
  }

  protected removeRubricRow(index: number): void {
    if (this.rubric.length > 1) this.rubric.removeAt(index);
  }

  protected saveAssignment(): void {
    const moduleId = this.assignmentModuleId();
    if (!moduleId || this.assignmentForm.invalid || this.busy()) {
      this.assignmentForm.markAllAsTouched();
      return;
    }
    const value = this.assignmentForm.getRawValue();
    const submissionTypes = [
      value.textResponse ? 'text' : null,
      value.photoUpload ? 'photo' : null,
      value.link ? 'link' : null,
      value.fileUpload ? 'file' : null,
    ].filter((item): item is string => item !== null);
    if (submissionTypes.length === 0) {
      this.errorMessage.set('Choose at least one submission type.');
      return;
    }
    const request: SaveAssignment = {
      moduleId,
      lessonId: null,
      title: value.title.trim(),
      taskDescription: value.taskDescription.trim(),
      submissionTypes,
      dueDate: value.dueDate || null,
      gracePeriodDays: value.gracePeriodDays,
      allowLate: value.allowLate,
      requireMentorApproval: value.requireMentorApproval,
      rubric: value.rubric.map((criterion) => ({
        description: criterion.description.trim(),
        points: criterion.points,
      })),
    };
    const editingId = this.editingAssignmentId();
    const operation = editingId
      ? this.api.updateAssignment(editingId, request)
      : this.api.createAssignment(request);
    this.run(operation, editingId ? 'Assignment updated.' : 'Assignment created.', () => {
      this.closeAssignment();
      this.reload();
    });
  }

  protected publishAssignment(assignment: AssignmentWorkspace): void {
    this.run(
      this.api.publishAssignment(assignment.id, 'Published'),
      'Assignment published and assigned to enrolled youth.',
      () => this.reload(),
    );
  }

  protected retry(): void {
    this.loadTracks(this.selectedTrackId());
  }

  private loadTracks(preferredId?: string): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.api.tracks().subscribe({
      next: (tracks) => {
        this.tracks.set(tracks);
        const selected =
          (preferredId && tracks.some((track) => track.id === preferredId)
            ? preferredId
            : tracks[0]?.id) ?? '';
        this.selectedTrackId.set(selected);
        if (selected) {
          this.loadWorkspace(selected);
        } else {
          this.workspace.set(null);
          this.loading.set(false);
        }
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.fail(error);
      },
    });
  }

  private loadWorkspace(id: string): void {
    if (!id) return;
    this.loading.set(true);
    this.api
      .track(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (workspace) => this.workspace.set(workspace),
        error: (error: unknown) => this.fail(error),
      });
  }

  private reload(): void {
    const id = this.selectedTrackId();
    if (id) this.loadWorkspace(id);
  }

  private run(operation: Observable<unknown>, message: string, after?: () => void): void {
    if (this.busy()) return;
    this.start();
    operation.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => {
        this.outcome.set(message);
        after?.();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  private start(): void {
    this.busy.set(true);
    this.errorMessage.set(null);
    this.outcome.set(null);
  }

  private fail(error: unknown): void {
    this.busy.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }

  private quizRequest(moduleId: string | null, lessonId: string | null): SaveQuiz {
    const value = this.quizForm.getRawValue();
    return {
      moduleId,
      lessonId,
      passMark: value.passMark,
      attemptsAllowed: value.attemptsAllowed,
      feedbackMode: value.feedbackMode,
      shuffle: value.shuffle,
      requiredToCompleteModule: value.requiredToCompleteModule,
    };
  }

  private rubricRow(description = '', points = 1) {
    return this.fb.nonNullable.group({
      description: [description, Validators.required],
      points: [points, [Validators.required, Validators.min(1)]],
    });
  }

  private resetAssignmentForm(): void {
    this.rubric.clear();
    this.rubric.push(this.rubricRow());
    this.assignmentForm.reset({
      title: '',
      taskDescription: '',
      textResponse: false,
      photoUpload: false,
      link: false,
      fileUpload: false,
      dueDate: '',
      gracePeriodDays: 0,
      allowLate: true,
      requireMentorApproval: true,
    });
  }

  private clean(value: string): string | null {
    const cleaned = value.trim();
    return cleaned || null;
  }

  private lines(value: string): string[] {
    return value
      .split(/\r?\n|,/)
      .map((item) => item.trim())
      .filter(Boolean);
  }
}
