import { DatePipe, DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, AssignmentBrief, AssignmentUpload, LearningApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-LEARN-006, L2-NFR-001
@Component({
  selector: 'wu-youth-assignment',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, ReactiveFormsModule, DatePipe],
  templateUrl: './youth-assignment.html',
})
export class YouthAssignmentPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningApi);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly doc = inject(DOCUMENT);
  private readonly assignmentId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly brief = signal<AssignmentBrief | undefined>(undefined);
  protected readonly upload = signal<AssignmentUpload | undefined>(undefined);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fileError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    bodyText: ['', Validators.maxLength(12000)],
    linkUrl: ['', Validators.pattern(/^https?:\/\/.+/i)],
    attest: [false, Validators.requiredTrue],
  });

  constructor() {
    this.api.assignmentBrief(this.assignmentId).subscribe({
      next: (brief) => this.brief.set(brief),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected photoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const brief = this.brief();
    const extension = file.name.split('.').pop()?.toLowerCase();
    if (
      !brief ||
      !['image/png', 'image/jpeg'].includes(file.type) ||
      !['png', 'jpg', 'jpeg'].includes(extension ?? '')
    ) {
      this.fileError.set('Choose a JPG or PNG image.');
      return;
    }
    if (file.size > brief.maxUploadBytes) {
      this.fileError.set('The image must be 10 MB or smaller.');
      return;
    }

    this.fileError.set(null);
    this.statusMessage.set('Uploading photo…');
    this.api.uploadAssignmentPhoto(this.assignmentId, file).subscribe({
      next: (upload) => {
        this.upload.set(upload);
        this.statusMessage.set(`Photo ready: ${upload.fileName}`);
      },
      error: (error: unknown) => {
        this.statusMessage.set(null);
        this.fileError.set(ApiError.from(error).title);
      },
    });
  }

  protected submit(): void {
    const brief = this.brief();
    const value = this.form.getRawValue();
    if (!brief || this.saving() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (!value.bodyText.trim() && !value.linkUrl.trim() && !this.upload()) {
      this.errorMessage.set('Provide at least one of the allowed submission types.');
      return;
    }
    if (
      !this.doc.defaultView?.confirm(
        'Submit this assignment? Your submitted copy will be read-only unless your mentor requests a revision.',
      )
    )
      return;

    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .submitAssignment(this.assignmentId, {
        bodyText: value.bodyText.trim() || null,
        photoUploadId: this.upload()?.id ?? null,
        photoUrl: null,
        linkUrl: value.linkUrl.trim() || null,
        isCodingProject: false,
        youthNote: null,
        attest: value.attest,
      })
      .subscribe({
        next: (result) => void this.router.navigate(['/youth/submissions', result.id]),
        error: (error: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }
}
