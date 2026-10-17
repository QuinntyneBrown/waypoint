import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  ApiError,
  CurriculumAuthoringService,
  ProgressionCompetency,
  ProgressionStage,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WAccessibleDialog,
  WButton,
  WFormField,
  WLabel,
  WShell,
  WTextarea,
  WTextField,
  WAlert,
  WCard,
  WCardBody,
} from '@waypoint/components';
import { finalize, Observable } from 'rxjs';
import {
  EditorDraftService,
  UnsavedChangesAware,
  DraftSaveResult,
  UnsavedChangesService,
} from '@waypoint/core';

type ProgressionAuthoringDraft =
  | {
      kind: 'stage';
      editingStageId: string | null;
      value: { name: string; description: string };
    }
  | {
      kind: 'competency';
      stageId: string;
      value: { name: string };
    };

// Traces to: L2-CURRIC-006, L2-NFR-001, L2-NFR-009, L2-NFR-010
@Component({
  selector: 'w-progression-builder',
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
    WAlert,
    WCard,
    WCardBody,
  ],
  templateUrl: './progression-builder.html',
  styleUrl: './curriculum-admin.css',
})
export class ProgressionBuilderPage implements UnsavedChangesAware {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(CurriculumAuthoringService);
  private readonly fb = inject(FormBuilder);
  private readonly drafts = inject(EditorDraftService);
  private readonly unsavedChanges = inject(UnsavedChangesService);
  private readonly draftKey = 'admin.progression';

  protected readonly stages = signal<ProgressionStage[]>([]);
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);
  protected readonly stageDialogOpen = signal(false);
  protected readonly editingStageId = signal<string | null>(null);
  protected readonly competencyStageId = signal<string | null>(null);

  protected readonly stageForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    description: ['', Validators.maxLength(2000)],
  });
  protected readonly competencyForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
  });

  constructor() {
    this.restoreDraft();
    this.load();
  }

  canDeactivate(): DraftSaveResult {
    return this.unsavedChanges.confirmSaveOrStay(this.isDirty(), 'authoring', () =>
      this.saveLocalDraft(true),
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    this.unsavedChanges.warnBeforeUnload(event, this.isDirty(), () => this.saveLocalDraft(false));
  }

  protected openStage(stage?: ProgressionStage): void {
    this.editingStageId.set(stage?.id ?? null);
    this.stageForm.reset({
      name: stage?.name ?? '',
      description: stage?.description ?? '',
    });
    this.stageDialogOpen.set(true);
  }

  protected closeStage(): void {
    this.stageDialogOpen.set(false);
    this.editingStageId.set(null);
  }

  protected saveStage(): void {
    if (this.stageForm.invalid || this.busy()) {
      this.stageForm.markAllAsTouched();
      return;
    }
    const value = this.stageForm.getRawValue();
    const editingId = this.editingStageId();
    this.start();
    const operation: Observable<unknown> = editingId
      ? this.api.updateStage(editingId, {
          name: value.name.trim(),
          description: this.clean(value.description),
        })
      : this.api.createStage({
          name: value.name.trim(),
          description: this.clean(value.description),
          order: this.stages().length + 1,
        });
    operation.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => {
        this.drafts.clear(this.draftKey);
        this.outcome.set(editingId ? 'Stage updated.' : 'Stage added.');
        this.closeStage();
        this.loadStages();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected moveStage(id: string, direction: -1 | 1): void {
    const ids = this.stages().map((stage) => stage.id);
    const current = ids.indexOf(id);
    const target = current + direction;
    if (current < 0 || target < 0 || target >= ids.length || this.busy()) return;
    [ids[current], ids[target]] = [ids[target], ids[current]];
    this.start();
    this.api
      .reorderStages(ids)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.outcome.set('Framework order saved.');
          this.loadStages();
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected openCompetency(stageId: string): void {
    this.competencyForm.reset({ name: '' });
    this.competencyStageId.set(stageId);
  }

  protected closeCompetency(): void {
    this.competencyStageId.set(null);
  }

  protected addCompetency(): void {
    const stageId = this.competencyStageId();
    if (!stageId || this.competencyForm.invalid || this.busy()) {
      this.competencyForm.markAllAsTouched();
      return;
    }
    this.start();
    this.api
      .addCompetency(stageId, this.competencyForm.controls.name.value.trim())
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.drafts.clear(this.draftKey);
          this.outcome.set('Competency added.');
          this.closeCompetency();
          this.loadStages();
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected removeCompetency(competency: ProgressionCompetency): void {
    if (this.busy()) return;
    this.start();
    this.api
      .removeCompetency(competency.id)
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: () => {
          this.outcome.set(competency.name + ' removed from the active framework.');
          this.loadStages();
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
      .progression()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (stages) => this.stages.set(stages),
        error: (error: unknown) => this.fail(error),
      });
  }

  private loadStages(): void {
    this.api.progression().subscribe({
      next: (stages) => this.stages.set(stages),
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

  private clean(value: string): string | null {
    const cleaned = value.trim();
    return cleaned || null;
  }

  private isDirty(): boolean {
    return (
      (this.stageDialogOpen() && this.stageForm.dirty) ||
      (this.competencyStageId() !== null && this.competencyForm.dirty)
    );
  }

  private saveLocalDraft(markPristine: boolean): boolean {
    let draft: ProgressionAuthoringDraft | null = null;
    if (this.stageDialogOpen() && this.stageForm.dirty) {
      draft = {
        kind: 'stage',
        editingStageId: this.editingStageId(),
        value: this.stageForm.getRawValue(),
      };
    } else if (this.competencyStageId() && this.competencyForm.dirty) {
      draft = {
        kind: 'competency',
        stageId: this.competencyStageId()!,
        value: this.competencyForm.getRawValue(),
      };
    }
    if (!draft) return true;
    const saved = this.drafts.save(this.draftKey, draft);
    if (saved && markPristine) {
      this.stageForm.markAsPristine();
      this.competencyForm.markAsPristine();
    }
    return saved;
  }

  private restoreDraft(): void {
    const draft = this.drafts.take<ProgressionAuthoringDraft>(this.draftKey);
    if (!draft) return;
    if (draft.kind === 'stage') {
      this.editingStageId.set(draft.editingStageId);
      this.stageForm.reset(draft.value);
      this.stageForm.markAsDirty();
      this.stageDialogOpen.set(true);
    } else {
      this.competencyForm.reset(draft.value);
      this.competencyForm.markAsDirty();
      this.competencyStageId.set(draft.stageId);
    }
    this.outcome.set('Authoring draft restored.');
  }
}
