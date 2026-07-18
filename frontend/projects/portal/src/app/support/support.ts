import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiError, SUPPORT_CATEGORIES, SupportApi } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuIcon, WuShell } from '@wu/ui';
import { shellRole } from '../shell-role';

// Traces to: L2-PLATFORM-005
@Component({
  selector: 'wu-support',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, ReactiveFormsModule, WuIcon],
  templateUrl: './support.html',
})
export class Support {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(SupportApi);
  private readonly auth = inject(AuthStore);

  protected readonly categories = SUPPORT_CATEGORIES;
  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');

  protected readonly form = this.fb.nonNullable.group({
    category: [SUPPORT_CATEGORIES[0], [Validators.required]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    replyToEmail: [this.auth.user()?.email ?? '', [Validators.required, Validators.email]],
  });
  protected readonly submitting = signal(false);
  protected readonly done = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected submit(): void {
    this.errorMessage.set(null);
    if (this.form.invalid) {
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
}
