import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { WButton, WStatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-006
@Component({
  selector: 'w-forbidden',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink, WButton],
  template: `<w-state-page
    icon="lock"
    tone="warning"
    eyebrow="Error 403"
    title="You don’t have access to that"
    text="This area is for a different role. If you think this is a mistake, switch to the right account or head back to your home."
  >
    <a w-button variant="primary" [routerLink]="home">Back to your home</a>
    <button w-button variant="ghost" type="button" (click)="switchAccount()">Switch account</button>
  </w-state-page>`,
})
export class Forbidden {
  private readonly auth = inject(AuthStore);
  protected readonly home = roleHome(this.auth.role());
  protected switchAccount(): void {
    this.auth.signOut();
  }
}
