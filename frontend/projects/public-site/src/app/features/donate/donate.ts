import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ApiError, PublicSiteService } from '@waypoint/api';
import { WIcon } from '@waypoint/components';

function isEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

// Traces to: L2-PUB-003
@Component({
  selector: 'w-donate',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, WIcon],
  templateUrl: './donate.html',
})
export class Donate {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(PublicSiteService);

  protected readonly tiers = [20, 50, 100] as const;
  protected readonly selected = signal<number | null>(null);
  protected readonly confirmed = signal(false);
  protected readonly submitting = signal(false);
  protected readonly showErrors = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    donorName: [''],
    email: [''],
  });

  private readonly rawValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });
  private readonly value = computed(() => {
    const v = this.rawValue();
    return { donorName: v.donorName ?? '', email: v.email ?? '' };
  });

  protected readonly errors = computed<Record<string, string>>(() => {
    const v = this.value();
    const e: Record<string, string> = {};
    if (!v.donorName.trim()) e['donorName'] = 'Enter your name.';
    if (!isEmail(v.email)) e['email'] = 'Enter a valid email address.';
    return e;
  });

  protected error(field: string): string {
    return this.showErrors() ? (this.errors()[field] ?? '') : '';
  }

  /**
   * Handoff URL to the external PCI-compliant provider. No card data is ever
   * collected or stored by this app (L2-PUB-003 AC3).
   */
  protected readonly providerUrl = computed(
    () =>
      `https://donate.example-pci-provider.org/waypoint?interval=monthly&amount=${this.selected() ?? ''}`,
  );

  protected select(amount: number): void {
    this.selected.set(amount);
    this.confirmed.set(false);
  }

  protected confirm(): void {
    const amount = this.selected();
    if (amount === null) return;
    this.serverError.set(null);
    this.showErrors.set(true);
    if (Object.keys(this.errors()).length > 0) return;

    this.submitting.set(true);
    const v = this.form.getRawValue();
    // Record the donor's pledge intent server-side; payment (and any provider
    // reference) is handled later by the external PCI provider — never here.
    this.api
      .submitDonation({
        donorName: v.donorName.trim(),
        email: v.email.trim(),
        tier: `$${amount}/mo`,
        monthlyAmount: amount,
        frequency: 'monthly',
        providerReference: null,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.confirmed.set(true);
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          this.serverError.set(ApiError.from(err).title);
        },
      });
  }
}
