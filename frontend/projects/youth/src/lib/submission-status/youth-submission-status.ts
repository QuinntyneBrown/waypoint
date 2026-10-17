import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthSubmissionStatus } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WButton, WLink, WShell, WAlert, WCard, WCardBody } from '@waypoint/components';

// Traces to: L2-LEARN-007
@Component({
  selector: 'w-youth-submission-status',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, WButton, WLink, WAlert, WCard, WCardBody],
  templateUrl: './youth-submission-status.html',
  styles: `
    .coding-sample {
      overflow-x: auto;
      white-space: pre-wrap;
      padding: var(--w-s4);
      background: #17202a;
      color: #f8f9f9;
      border-radius: var(--w-r-input);
    }
  `,
})
export class YouthSubmissionStatusPage implements OnDestroy {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly route = inject(ActivatedRoute);
  private readonly doc = inject(DOCUMENT);
  private readonly submissionId = this.route.snapshot.paramMap.get('id') ?? '';
  private photoObjectUrl: string | undefined;
  private fileObjectUrl: string | undefined;

  protected readonly submission = signal<YouthSubmissionStatus | undefined>(undefined);
  protected readonly photoUrl = signal<string | undefined>(undefined);
  protected readonly fileUrl = signal<string | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.submissionStatus(this.submissionId).subscribe({
      next: (submission) => {
        this.submission.set(submission);
        if (submission.photoUrl) this.loadPhoto(submission.photoUrl);
        if (submission.file) this.loadFile(submission.file.url);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected displayStatus(status: string): string {
    if (status === 'NeedsRevision') return 'Needs revision';
    if (status === 'UnderReview') return 'Under review';
    return status;
  }

  ngOnDestroy(): void {
    if (this.photoObjectUrl) this.doc.defaultView?.URL.revokeObjectURL(this.photoObjectUrl);
    if (this.fileObjectUrl) this.doc.defaultView?.URL.revokeObjectURL(this.fileObjectUrl);
  }

  private loadPhoto(path: string): void {
    this.api.submissionFile(path).subscribe({
      next: (blob) => {
        this.photoObjectUrl = this.doc.defaultView?.URL.createObjectURL(blob);
        this.photoUrl.set(this.photoObjectUrl);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadFile(path: string): void {
    this.api.submissionFile(path).subscribe({
      next: (blob) => {
        this.fileObjectUrl = this.doc.defaultView?.URL.createObjectURL(blob);
        this.fileUrl.set(this.fileObjectUrl);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
