import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, CodingLesson, CodingLessonStep, LearningService } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WErrorText,
  WFormField,
  WLabel,
  WShell,
  WTextarea,
  WAlert,
  WCard,
  WCardBody,
} from '@waypoint/components';

// Traces to: L2-LEARN-008
@Component({
  selector: 'w-youth-coding',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    RouterLink,
    ReactiveFormsModule,
    WButton,
    WFormField,
    WLabel,
    WErrorText,
    WTextarea,
    WAlert,
    WCard,
    WCardBody,
  ],
  templateUrl: './youth-coding.html',
  styles: `
    .coding-layout {
      display: grid;
      grid-template-columns: minmax(15rem, 0.7fr) minmax(0, 1.6fr);
      gap: var(--w-s5);
      align-items: start;
    }
    .coding-step-button {
      width: 100%;
      text-align: left;
      justify-content: flex-start;
      white-space: normal;
    }
    .coding-sample,
    .coding-editor {
      font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
      tab-size: 2;
    }
    .coding-sample {
      overflow-x: auto;
      white-space: pre-wrap;
      padding: var(--w-s4);
      background: #17202a;
      color: #f8f9f9;
      border-radius: var(--w-r-input);
    }
    .coding-editor {
      width: 100%;
      min-height: 18rem;
      resize: vertical;
    }
    .coding-preview {
      display: block;
      width: 100%;
      min-height: 18rem;
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-input);
      background: white;
    }
    @media (max-width: 50rem) {
      .coding-layout {
        grid-template-columns: minmax(0, 1fr);
      }
      .coding-editor,
      .coding-preview {
        min-height: 15rem;
      }
    }
  `,
})
export class YouthCodingPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly doc = inject(DOCUMENT);
  private readonly lessonId = this.route.snapshot.paramMap.get('id') ?? '';
  private saveInFlight = false;
  private saveQueued = false;

  protected readonly lesson = signal<CodingLesson | undefined>(undefined);
  protected readonly code = signal('');
  protected readonly checkedStepIds = signal<Set<string>>(new Set());
  protected readonly currentStepIndex = signal(0);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly saveStatus = signal<string | null>(null);
  protected readonly submitting = signal(false);
  protected readonly currentStep = computed<CodingLessonStep | undefined>(
    () => this.lesson()?.steps[this.currentStepIndex()],
  );
  protected readonly completionPercent = computed(() => {
    const total = this.lesson()?.steps.length ?? 0;
    return total === 0 ? 0 : Math.round((this.checkedStepIds().size / total) * 100);
  });
  protected readonly preview = computed<SafeHtml>(() => {
    const policy = "default-src 'none'; style-src 'unsafe-inline'; img-src data:; font-src data:";
    const document = `<!doctype html><html><head><meta charset="utf-8"><meta http-equiv="Content-Security-Policy" content="${policy}"><style>body{font-family:system-ui,sans-serif;padding:1rem}</style></head><body>${this.code()}</body></html>`;
    return this.sanitizer.bypassSecurityTrustHtml(document);
  });
  protected readonly submitForm = this.fb.nonNullable.group({
    mentorNote: ['', Validators.maxLength(1000)],
    attest: [false, Validators.requiredTrue],
  });

  constructor() {
    this.api.codingLesson(this.lessonId).subscribe({
      next: (lesson) => {
        this.lesson.set(lesson);
        this.code.set(lesson.code);
        this.checkedStepIds.set(new Set(lesson.checkedStepIds));
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected selectStep(index: number): void {
    this.currentStepIndex.set(index);
  }

  protected updateCode(event: Event): void {
    this.code.set((event.target as HTMLTextAreaElement).value);
    this.queueSave();
  }

  protected toggleStep(stepId: string, checked: boolean): void {
    this.checkedStepIds.update((current) => {
      const updated = new Set(current);
      if (checked) updated.add(stepId);
      else updated.delete(stepId);
      return updated;
    });
    this.queueSave();
  }

  protected isChecked(stepId: string): boolean {
    return this.checkedStepIds().has(stepId);
  }

  protected submitProject(): void {
    const lesson = this.lesson();
    if (!lesson || this.submitForm.invalid || this.submitting()) {
      this.submitForm.markAllAsTouched();
      return;
    }
    if (!this.code().trim()) {
      this.errorMessage.set('Add some code before submitting your project.');
      return;
    }
    if (
      !this.doc.defaultView?.confirm(
        'Submit this coding project? Your mentor will review the submitted copy.',
      )
    )
      return;

    const form = this.submitForm.getRawValue();
    this.submitting.set(true);
    this.errorMessage.set(null);
    this.api
      .submitCodingProject(this.lessonId, {
        code: this.code(),
        mentorNote: form.mentorNote.trim() || null,
        attest: form.attest,
      })
      .subscribe({
        next: (result) => void this.router.navigate(['/youth/submissions', result.id]),
        error: (error: unknown) => {
          this.submitting.set(false);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }

  private queueSave(): void {
    this.saveQueued = true;
    this.flushSave();
  }

  private flushSave(): void {
    if (this.saveInFlight || !this.saveQueued || !this.lesson()) return;
    this.saveInFlight = true;
    this.saveQueued = false;
    this.saveStatus.set('Saving coding workspace…');
    this.api
      .saveCodingProgress(this.lessonId, {
        code: this.code(),
        checkedStepIds: [...this.checkedStepIds()],
      })
      .subscribe({
        next: (result) => {
          this.saveInFlight = false;
          this.lesson.update((lesson) =>
            lesson
              ? {
                  ...lesson,
                  completed: result.completed,
                  completionPercent: result.completionPercent,
                  badge: lesson.badge
                    ? {
                        ...lesson.badge,
                        completedCriteria: result.completedCriteria,
                        totalCriteria: result.totalCriteria,
                        earned: result.badgeEarned,
                      }
                    : null,
                }
              : lesson,
          );
          if (this.saveQueued) this.flushSave();
          else
            this.saveStatus.set(
              result.completed ? 'Coding lesson complete. Workspace saved.' : 'Workspace saved.',
            );
        },
        error: (error: unknown) => {
          this.saveInFlight = false;
          this.saveStatus.set(null);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }
}
