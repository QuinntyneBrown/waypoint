import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnDestroy,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiError, SUPPORT_CATEGORIES, SupportService, SupportScreenshot } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WErrorText,
  WFormField,
  WHelp,
  WIcon,
  WLabel,
  WShell,
} from '@waypoint/components';
import { shellRole } from '@waypoint/core';

// Traces to: L2-PLATFORM-005
@Component({
  selector: 'w-support',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    ReactiveFormsModule,
    RouterLink,
    WIcon,
    WButton,
    WFormField,
    WLabel,
    WHelp,
    WErrorText,
  ],
  templateUrl: './support.html',
})
export class Support implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(SupportService);
  private readonly auth = inject(AuthStore);
  private readonly document = inject(DOCUMENT);
  private previewObjectUrl: string | undefined;

  protected readonly categories = SUPPORT_CATEGORIES;
  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');

  protected readonly form = this.fb.nonNullable.group({
    category: [SUPPORT_CATEGORIES[0], [Validators.required]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    replyToEmail: [this.auth.user()?.email ?? '', [Validators.required, Validators.email]],
  });
  protected readonly submitting = signal(false);
  protected readonly uploading = signal(false);
  protected readonly done = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly fileError = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly screenshot = signal<SupportScreenshot | null>(null);
  protected readonly screenshotPreview = signal<string | null>(null);

  protected screenshotSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const extension = file.name.split('.').pop()?.toLowerCase();
    if (
      !['image/png', 'image/jpeg'].includes(file.type) ||
      !['png', 'jpg', 'jpeg'].includes(extension ?? '')
    ) {
      this.fileError.set('Choose a JPG or PNG image.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.fileError.set('The screenshot must be 5 MB or smaller.');
      return;
    }

    this.fileError.set(null);
    this.statusMessage.set('Uploading screenshot…');
    this.uploading.set(true);
    this.api.uploadScreenshot(file).subscribe({
      next: (screenshot) => {
        this.uploading.set(false);
        this.screenshot.set(screenshot);
        this.setPreview(file);
        this.statusMessage.set(`Screenshot ready: ${screenshot.fileName}`);
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        this.statusMessage.set(null);
        const apiError = ApiError.from(error);
        this.fileError.set(apiError.fieldErrors[0]?.errorMessage ?? apiError.title);
      },
    });
  }

  protected removeScreenshot(): void {
    const screenshot = this.screenshot();
    if (!screenshot || this.uploading()) return;
    this.uploading.set(true);
    this.fileError.set(null);
    this.api.removeScreenshot(screenshot.id).subscribe({
      next: () => {
        this.uploading.set(false);
        this.screenshot.set(null);
        this.clearPreview();
        this.statusMessage.set('Screenshot removed.');
      },
      error: (error: unknown) => {
        this.uploading.set(false);
        this.fileError.set(ApiError.from(error).title);
      },
    });
  }

  protected submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid || this.uploading()) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    const v = this.form.getRawValue();
    this.api
      .submit({
        category: v.category,
        description: v.description.trim(),
        replyToEmail: v.replyToEmail.trim(),
        screenshotFileId: this.screenshot()?.id ?? null,
      })
      .subscribe({
        next: () => this.done.set(true),
        error: (err: unknown) => {
          this.submitting.set(false);
          this.errorMessage.set(ApiError.from(err).title);
        },
      });
  }

  protected signOut(): void {
    this.auth.signOut();
  }

  ngOnDestroy(): void {
    this.clearPreview();
  }

  private setPreview(file: File): void {
    this.clearPreview();
    this.previewObjectUrl = this.document.defaultView?.URL.createObjectURL(file);
    this.screenshotPreview.set(this.previewObjectUrl ?? null);
  }

  private clearPreview(): void {
    if (this.previewObjectUrl) {
      this.document.defaultView?.URL.revokeObjectURL(this.previewObjectUrl);
    }
    this.previewObjectUrl = undefined;
    this.screenshotPreview.set(null);
  }
}
