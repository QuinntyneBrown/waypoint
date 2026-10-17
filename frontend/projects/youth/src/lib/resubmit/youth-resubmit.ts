import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthSubmissionStatus } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WFormField, WLabel, WShell, WTextarea, WTextField } from '@waypoint/components';

// Traces to: L2-LEARN-007
@Component({
  selector: 'w-youth-resubmit',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    RouterLink,
    ReactiveFormsModule,
    WButton,
    WFormField,
    WLabel,
    WTextField,
    WTextarea,
  ],
  templateUrl: './youth-resubmit.html',
})
export class YouthResubmitPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly submissionId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly submission = signal<YouthSubmissionStatus | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly conflictMessage = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    bodyText: ['', [Validators.required, Validators.maxLength(12000)]],
    linkUrl: ['', Validators.pattern(/^https?:\/\/.+/i)],
  });

  constructor() {
    this.load();
  }

  protected submit(): void {
    const submission = this.submission();
    if (!submission || this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.conflictMessage.set(null);
    this.api
      .resubmitAssignment(this.submissionId, {
        bodyText: value.bodyText.trim(),
        photoUrl: null,
        linkUrl: value.linkUrl.trim() || null,
        expectedVersion: submission.version,
      })
      .subscribe({
        next: () => void this.router.navigate(['/youth/submissions', this.submissionId]),
        error: (error: unknown) => {
          this.saving.set(false);
          const apiError = ApiError.from(error);
          if (apiError.status === 409) {
            this.conflictMessage.set(
              'This submission changed on the server. Your revision is still here. Review the latest feedback before resubmitting.',
            );
          } else {
            this.errorMessage.set(apiError.title);
          }
        },
      });
  }

  protected reviewLatest(): void {
    this.conflictMessage.set(null);
    this.errorMessage.set(null);
    this.load(true);
  }

  private load(preserveDraft = false): void {
    this.api.submissionStatus(this.submissionId).subscribe({
      next: (submission) => {
        this.submission.set(submission);
        if (!preserveDraft) {
          this.form.setValue({
            bodyText: submission.bodyText ?? '',
            linkUrl: submission.linkUrl ?? '',
          });
        }
        this.conflictMessage.set(null);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
