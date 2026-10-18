import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ApiError, ConsentService } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { Button, StatePage, Alert } from '@waypoint/components';

// Traces to: L2-CONSENT-002
@Component({
  selector: 'w-consent-required',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, Button, Alert],
  templateUrl: './consent-required.html',
  styleUrl: './consent-required.css',
})
export class ConsentRequired {
  private readonly api = inject(ConsentService);
  private readonly auth = inject(AuthStore);
  protected readonly notifying = signal(false);
  protected readonly deliveryMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);

  protected notifyGuardian(): void {
    if (this.notifying()) return;
    this.notifying.set(true);
    this.deliveryMessage.set(null);
    this.errorMessage.set(null);
    this.api.requestGuardian().subscribe({
      next: (delivery) => {
        this.notifying.set(false);
        this.deliveryMessage.set(
          `A secure consent link was sent by ${delivery.channel} to ${delivery.destination}.`,
        );
      },
      error: (error: unknown) => {
        this.notifying.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  protected guardianSignNow(): void {
    this.auth.signOut('/auth/sign-in?returnUrl=%2Fguardian%2Fconsent');
  }

  protected signOut(): void {
    this.auth.signOut();
  }
}
