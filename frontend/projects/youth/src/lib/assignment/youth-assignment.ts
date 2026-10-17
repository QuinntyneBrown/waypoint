import { DatePipe, DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiError, AssignmentBrief, AssignmentUpload, LearningService } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WErrorText,
  WFormField,
  WHelp,
  WLabel,
  WShell,
  WTextarea,
  WTextField,
  WAlert,
  WCard,
  WCardBody,
} from '@waypoint/components';

// Traces to: L2-LEARN-006, L2-NFR-001
@Component({
  selector: 'w-youth-assignment',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    RouterLink,
    ReactiveFormsModule,
    DatePipe,
    WButton,
    WFormField,
    WLabel,
    WHelp,
    WErrorText,
    WTextField,
    WTextarea,
    WAlert,
    WCard,
    WCardBody,
  ],
  templateUrl: './youth-assignment.html',
})
export class YouthAssignmentPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly doc = inject(DOCUMENT);
  private readonly assignmentId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly brief = signal<AssignmentBrief | undefined>(undefined);
  protected readonly photoUpload = signal<AssignmentUpload | undefined>(undefined);
  protected readonly fileUpload = signal<AssignmentUpload | undefined>(undefined);
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
    this.api.uploadAssignmentEvidence(this.assignmentId, file).subscribe({
      next: (upload) => {
        this.photoUpload.set(upload);
        this.statusMessage.set(`Photo ready: ${upload.fileName}`);
      },
      error: (error: unknown) => {
        this.statusMessage.set(null);
        const apiError = ApiError.from(error);
        this.fileError.set(apiError.fieldErrors[0]?.errorMessage ?? apiError.title);
      },
    });
  }

  protected fileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const brief = this.brief();
    if (!brief || file.type !== 'application/pdf' || !file.name.toLowerCase().endsWith('.pdf')) {
      this.fileError.set('Choose a PDF file.');
      return;
    }
    if (file.size > brief.maxUploadBytes) {
      this.fileError.set('The file must be 10 MB or smaller.');
      return;
    }

    this.fileError.set(null);
    this.statusMessage.set('Uploading file…');
    this.api.uploadAssignmentEvidence(this.assignmentId, file).subscribe({
      next: (upload) => {
        this.fileUpload.set(upload);
        this.statusMessage.set(`File ready: ${upload.fileName}`);
      },
      error: (error: unknown) => {
        this.statusMessage.set(null);
        const apiError = ApiError.from(error);
        this.fileError.set(apiError.fieldErrors[0]?.errorMessage ?? apiError.title);
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
    if (
      !value.bodyText.trim() &&
      !value.linkUrl.trim() &&
      !this.photoUpload() &&
      !this.fileUpload()
    ) {
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
        photoUploadId: this.photoUpload()?.id ?? null,
        fileUploadId: this.fileUpload()?.id ?? null,
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
