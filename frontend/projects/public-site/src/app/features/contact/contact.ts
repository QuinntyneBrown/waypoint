import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiError, PublicSiteApi } from '@waypoint/api';
import { WIcon } from '@waypoint/components';

function isEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

// Traces to: L2-PUB-003
@Component({
  selector: 'w-contact',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, WIcon],
  templateUrl: './contact.html',
})
export class Contact {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PublicSiteApi);

  protected readonly topics = [
    'General question',
    'Volunteering',
    'Partnership',
    'Donation',
    'Feedback',
  ] as const;

  protected readonly form = this.fb.nonNullable.group({
    name: [''],
    email: [''],
    topic: [this.topics[0] as string],
    message: [''],
  });

  private readonly rawValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });
  private readonly value = computed(() => {
    const v = this.rawValue();
    return {
      name: v.name ?? '',
      email: v.email ?? '',
      topic: v.topic ?? '',
      message: v.message ?? '',
    };
  });

  protected readonly showErrors = signal(false);
  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly done = signal(false);

  protected readonly errors = computed<Record<string, string>>(() => {
    const v = this.value();
    const e: Record<string, string> = {};
    if (!v.name.trim()) e['name'] = 'Enter your name.';
    if (!isEmail(v.email)) e['email'] = 'Enter a valid email address.';
    if (!v.topic.trim()) e['topic'] = 'Choose a topic.';
    if (!v.message.trim()) e['message'] = 'Enter a message.';
    return e;
  });

  protected readonly isValid = computed(() => Object.keys(this.errors()).length === 0);

  protected error(field: string): string {
    return this.showErrors() ? (this.errors()[field] ?? '') : '';
  }

  protected submit(): void {
    this.serverError.set(null);
    this.showErrors.set(true);
    if (!this.isValid()) return;
    this.submitting.set(true);
    const v = this.form.getRawValue();
    this.api
      .submitContact({
        name: v.name.trim(),
        email: v.email.trim(),
        topic: v.topic,
        message: v.message.trim(),
      })
      .subscribe({
        next: () => this.done.set(true),
        error: (err: unknown) => {
          this.submitting.set(false);
          this.serverError.set(ApiError.from(err).title);
        },
      });
  }
}
