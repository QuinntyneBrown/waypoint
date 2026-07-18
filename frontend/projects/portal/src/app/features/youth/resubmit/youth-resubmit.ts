import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, LearningApi, YouthSubmissionStatus } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-LEARN-007
@Component({
  selector: 'wu-youth-resubmit',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, ReactiveFormsModule],
  templateUrl: './youth-resubmit.html',
})
export class YouthResubmitPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningApi);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly submissionId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly submission = signal<YouthSubmissionStatus | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    bodyText: ['', [Validators.required, Validators.maxLength(12000)]],
    linkUrl: ['', Validators.pattern(/^https?:\/\/.+/i)],
  });

  constructor() {
    this.api.submissionStatus(this.submissionId).subscribe({
      next: (submission) => {
        this.submission.set(submission);
        this.form.setValue({ bodyText: submission.bodyText ?? '', linkUrl: submission.linkUrl ?? '' });
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected submit(): void {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    const value = this.form.getRawValue();
    this.saving.set(true);
    this.api.resubmitAssignment(this.submissionId, {
      bodyText: value.bodyText.trim(),
      photoUrl: null,
      linkUrl: value.linkUrl.trim() || null,
    }).subscribe({
      next: () => void this.router.navigate(['/youth/submissions', this.submissionId]),
      error: (error: unknown) => { this.saving.set(false); this.errorMessage.set(ApiError.from(error).title); },
    });
  }
}
