import {
  ChangeDetectionStrategy,
  Component,
  computed,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  BadgeDefinition,
  CurriculumAuthoringService,
  PeopleService,
  PersonSummary,
  ProgressionStage,
  SaveBadge,
  TrackWorkspace,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WAccessibleDialog,
  WButton,
  WFormField,
  WLabel,
  WSelect,
  WShell,
  WTextarea,
  WTextField,
  WAlert,
  WCard,
  WCardBody,
} from '@waypoint/components';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import {
  EditorDraftService,
  UnsavedChangesAware,
  DraftSaveResult,
  UnsavedChangesService,
} from '@waypoint/core';

interface BadgeTarget {
  id: string;
  label: string;
  rule: string;
}

// Traces to: L2-CURRIC-005, L2-NFR-001, L2-NFR-009, L2-NFR-010
@Component({
  selector: 'w-badge-builder',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WAccessibleDialog,
    ReactiveFormsModule,
    WButton,
    WFormField,
    WLabel,
    WTextField,
    WTextarea,
    WSelect,
    WAlert,
    WCard,
    WCardBody,
  ],
  templateUrl: './badge-builder.html',
  styleUrl: './curriculum-admin.css',
})
export class BadgeBuilderPage implements UnsavedChangesAware {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(CurriculumAuthoringService);
  private readonly peopleApi = inject(PeopleService);
  private readonly fb = inject(FormBuilder);
  private readonly drafts = inject(EditorDraftService);
  private readonly unsavedChanges = inject(UnsavedChangesService);
  private readonly draftKey = 'admin.badges';

  protected readonly badges = signal<BadgeDefinition[]>([]);
  protected readonly stages = signal<ProgressionStage[]>([]);
  protected readonly youth = signal<PersonSummary[]>([]);
  protected readonly allTargets = signal<BadgeTarget[]>([]);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly awardBadge = signal<BadgeDefinition | null>(null);
  protected readonly selectedRule = signal('module-completed');

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    icon: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', Validators.maxLength(2000)],
    criteriaKind: ['Auto' as SaveBadge['criteriaKind'], Validators.required],
    autoRule: ['module-completed'],
    targetRefId: [''],
    competencyId: [''],
  });
  protected readonly awardForm = this.fb.nonNullable.group({
    youthUserId: ['', Validators.required],
  });
  protected readonly targets = computed(() => {
    return this.allTargets().filter((target) => target.rule === this.selectedRule());
  });

  constructor() {
    this.restoreDraft();
    this.load();
  }

  canDeactivate(): DraftSaveResult {
    return this.unsavedChanges.confirmSaveOrStay(this.form.dirty, 'authoring', () =>
      this.saveLocalDraft(true),
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    this.unsavedChanges.warnBeforeUnload(event, this.form.dirty, () => this.saveLocalDraft(false));
  }

  protected criteriaChanged(): void {
    if (this.form.controls.criteriaKind.value === 'Manual') {
      this.form.patchValue({ autoRule: '', targetRefId: '' });
      this.selectedRule.set('');
    } else {
      this.form.patchValue({ autoRule: 'module-completed', targetRefId: '' });
      this.selectedRule.set('module-completed');
    }
  }

  protected ruleChanged(): void {
    this.selectedRule.set(this.form.controls.autoRule.value);
    this.form.controls.targetRefId.setValue('');
  }

  protected saveBadge(): void {
    if (this.form.invalid || this.busy()) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const isAutomatic = value.criteriaKind === 'Auto';
    const targetRequired =
      isAutomatic &&
      ['module-completed', 'lesson-completed', 'quiz-passed', 'assignment-approved'].includes(
        value.autoRule,
      );
    if (targetRequired && !value.targetRefId) {
      this.errorMessage.set('Choose the curriculum item that triggers this badge.');
      return;
    }
    this.start();
    this.api
      .createBadge({
        name: value.name.trim(),
        icon: value.icon.trim(),
        description: this.clean(value.description),
        criteriaKind: value.criteriaKind,
        autoRule: isAutomatic ? value.autoRule || null : null,
        targetRefId: isAutomatic ? value.targetRefId || null : null,
        competencyId: value.competencyId || null,
      })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.drafts.clear(this.draftKey);
          this.outcome.set('Badge definition saved.');
          this.form.reset({
            name: '',
            icon: '',
            description: '',
            criteriaKind: 'Auto',
            autoRule: 'module-completed',
            targetRefId: '',
            competencyId: '',
          });
          this.selectedRule.set('module-completed');
          this.loadBadges();
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected archive(badge: BadgeDefinition): void {
    if (this.busy()) return;
    this.start();
    this.api
      .archiveBadge(badge.id)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.badges.update((items) => items.filter((item) => item.id !== badge.id));
          this.outcome.set(badge.name + ' archived.');
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected openAward(badge: BadgeDefinition): void {
    this.awardForm.reset({ youthUserId: '' });
    this.awardBadge.set(badge);
  }

  protected confirmAward(): void {
    const badge = this.awardBadge();
    if (!badge || this.awardForm.invalid || this.busy()) {
      this.awardForm.markAllAsTouched();
      return;
    }
    const youthId = this.awardForm.controls.youthUserId.value;
    const youthName = this.youth().find((person) => person.id === youthId)?.displayName ?? 'Youth';
    this.start();
    this.api
      .awardBadge(badge.id, youthId)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.awardBadge.set(null);
          this.outcome.set(badge.name + ' awarded to ' + youthName + '.');
          this.loadBadges();
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected retry(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.api
      .tracks()
      .pipe(
        switchMap((tracks) => {
          const workspaces = tracks.length
            ? forkJoin(tracks.map((track) => this.api.track(track.id)))
            : of([] as TrackWorkspace[]);
          return forkJoin({
            badges: this.api.badges(),
            stages: this.api.progression(),
            people: this.peopleApi.list({ role: 'Youth', pageSize: 100 }),
            workspaces,
          });
        }),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: ({ badges, stages, people, workspaces }) => {
          this.badges.set(badges);
          this.stages.set(stages);
          this.youth.set(people.items);
          this.allTargets.set(this.buildTargets(workspaces));
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  private loadBadges(): void {
    this.api.badges().subscribe({
      next: (badges) => this.badges.set(badges),
      error: (error: unknown) => this.fail(error),
    });
  }

  private buildTargets(workspaces: TrackWorkspace[]): BadgeTarget[] {
    const targets: BadgeTarget[] = [];
    for (const track of workspaces) {
      for (const module of track.modules) {
        targets.push({ id: module.id, label: module.name, rule: 'module-completed' });
        for (const lesson of module.lessons) {
          targets.push({ id: lesson.id, label: lesson.title, rule: 'lesson-completed' });
          for (const quiz of lesson.quizzes) {
            targets.push({
              id: quiz.id,
              label: lesson.title + ' quiz',
              rule: 'quiz-passed',
            });
          }
          for (const assignment of lesson.assignments) {
            targets.push({
              id: assignment.id,
              label: assignment.title,
              rule: 'assignment-approved',
            });
          }
        }
        for (const quiz of module.quizzes) {
          targets.push({
            id: quiz.id,
            label: module.name + ' quiz',
            rule: 'quiz-passed',
          });
        }
        for (const assignment of module.assignments) {
          targets.push({
            id: assignment.id,
            label: assignment.title,
            rule: 'assignment-approved',
          });
        }
      }
    }
    return targets;
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

  private clean(value: string): string | null {
    const cleaned = value.trim();
    return cleaned || null;
  }

  private saveLocalDraft(markPristine: boolean): boolean {
    const saved = this.drafts.save(this.draftKey, this.form.getRawValue());
    if (saved && markPristine) this.form.markAsPristine();
    return saved;
  }

  private restoreDraft(): void {
    const draft = this.drafts.take<ReturnType<typeof this.form.getRawValue>>(this.draftKey);
    if (!draft) return;
    this.form.reset(draft);
    this.selectedRule.set(draft.autoRule);
    this.form.markAsDirty();
    this.outcome.set('Authoring draft restored.');
  }
}
