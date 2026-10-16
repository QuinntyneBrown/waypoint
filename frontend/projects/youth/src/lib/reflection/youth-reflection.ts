import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, LearningService, ReflectionEditor, SaveReflectionInput } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';
import { catchError, map, Observable, of, tap } from 'rxjs';
import {
  UnsavedChangesAware,
  FormDraftService,
  DraftSaveResult,
  UnsavedChangesService,
} from '@waypoint/core';

const OPTIONAL_PROMPT = 'One action I will take';

// Traces to: L2-LEARN-003, L2-NFR-007
@Component({
  selector: 'w-youth-reflection',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, ReactiveFormsModule],
  templateUrl: './youth-reflection.html',
  styles: `
    .mood-options {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(8rem, 1fr));
      gap: var(--w-sp-2);
    }
    .mood-option,
    .share-option {
      min-height: 2.75rem;
      display: flex;
      align-items: center;
      gap: var(--w-sp-2);
      cursor: pointer;
    }
    textarea {
      min-height: 10rem;
      resize: vertical;
    }
  `,
})
export class YouthReflectionPage implements UnsavedChangesAware {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly drafts = inject(FormDraftService);
  private readonly unsavedChanges = inject(UnsavedChangesService);
  private readonly lessonId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly editor = signal<ReflectionEditor | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(8000)]],
    mood: ['', Validators.required],
    secondaryResponse: ['', Validators.maxLength(4000)],
    shareWithMentor: false,
  });

  constructor() {
    this.api.reflectionEditor(this.lessonId).subscribe({
      next: (editor) => {
        this.editor.set(editor);
        if (editor.reflection) {
          this.form.setValue({
            text: editor.reflection.text ?? '',
            mood: editor.reflection.mood ?? '',
            secondaryResponse: editor.reflection.secondaryResponses[OPTIONAL_PROMPT] ?? '',
            shareWithMentor: editor.reflection.shareWithMentor,
          });
          this.form.markAsPristine();
          this.statusMessage.set(
            editor.reflection.isDraft ? 'Draft restored.' : 'Submitted reflection restored.',
          );
        }
        setTimeout(() => {
          if (this.drafts.restoreAndClear()) {
            this.form.markAsDirty();
            this.statusMessage.set('Draft restored after session expiry.');
          }
        });
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected saveDraft(): void {
    if (this.saving()) return;
    this.persist(true).subscribe();
  }

  protected submit(): void {
    if (this.saving() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.persist(false).subscribe();
  }

  canDeactivate(): DraftSaveResult {
    return this.unsavedChanges.confirmSaveOrStay(this.form.dirty, 'reflection', () =>
      this.persist(true).pipe(
        map(() => true),
        catchError(() => of(false)),
      ),
    );
  }

  @HostListener('window:beforeunload', ['$event'])
  protected beforeUnload(event: BeforeUnloadEvent): void {
    this.unsavedChanges.warnBeforeUnload(event, this.form.dirty);
  }

  private persist(isDraft: boolean): Observable<unknown> {
    this.saving.set(true);
    this.errorMessage.set(null);
    this.statusMessage.set(isDraft ? 'Saving draft…' : 'Submitting reflection…');
    return this.api.saveReflection(this.payload(isDraft)).pipe(
      tap(() => {
        this.saving.set(false);
        this.form.markAsPristine();
        this.statusMessage.set(isDraft ? 'Draft saved.' : 'Reflection submitted.');
      }),
      catchError((error: unknown) => {
        this.saving.set(false);
        this.statusMessage.set(null);
        this.errorMessage.set(ApiError.from(error).title);
        throw error;
      }),
    );
  }

  private payload(isDraft: boolean): SaveReflectionInput {
    const value = this.form.getRawValue();
    const secondaryResponse = value.secondaryResponse.trim();
    return {
      lessonId: this.lessonId,
      text: value.text.trim() || null,
      mood: value.mood || null,
      secondaryResponses: secondaryResponse ? { [OPTIONAL_PROMPT]: secondaryResponse } : {},
      shareWithMentor: value.shareWithMentor,
      isDraft,
    };
  }
}
