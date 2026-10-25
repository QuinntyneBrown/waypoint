import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { maskEmail, PasswordResetService } from '@waypoint/api';
import { ToastService, Button, StatePage } from '@waypoint/components';

// Traces to: L2-AUTH-005
@Component({
  selector: 'ph-reset-link-sent',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './reset-link-sent.html',
  styleUrl: './reset-link-sent.css',
})
export class ResetLinkSent {
  private readonly doc = inject(DOCUMENT);
  private readonly api = inject(PasswordResetService);
  private readonly toasts = inject(ToastService);
  private readonly email = (this.doc.defaultView?.history.state?.['email'] as string) ?? '';

  protected readonly bodyText = this.email
    ? `If an account exists for ${maskEmail(this.email)}, we’ve emailed a link to reset your password. The link expires in 1 hour.`
    : 'If an account exists for that address, we’ve emailed a link to reset your password. The link expires in 1 hour.';

  protected readonly resending = signal(false);

  protected resend(): void {
    if (!this.email || this.resending()) return;
    this.resending.set(true);
    this.api.requestReset({ email: this.email }).subscribe({
      next: () => {
        this.resending.set(false);
        this.toasts.success('Reset link sent again.');
      },
      error: () => {
        this.resending.set(false);
        this.toasts.error('Couldn’t resend just now. Please try again.');
      },
    });
  }
}
