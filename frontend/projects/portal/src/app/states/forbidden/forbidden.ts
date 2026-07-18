import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@wu/auth';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-006
@Component({
  selector: 'wu-forbidden',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="lock"
    tone="warning"
    eyebrow="Error 403"
    title="You don’t have access to that"
    text="This area is for a different role. If you think this is a mistake, switch to the right account or head back to your home."
  >
    <a class="wu-btn wu-btn--primary" [routerLink]="home">Back to your home</a>
    <button class="wu-btn wu-btn--ghost" type="button" (click)="switchAccount()">
      Switch account
    </button>
  </wu-state-page>`,
})
export class Forbidden {
  private readonly auth = inject(AuthStore);
  protected readonly home = roleHome(this.auth.role());
  protected switchAccount(): void {
    this.auth.signOut();
  }
}
