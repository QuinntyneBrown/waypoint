import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@wu/auth';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-007
@Component({
  selector: 'wu-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="compass"
    tone="info"
    eyebrow="Error 404"
    title="Page not found"
    text="The page you’re looking for moved or never existed. Let’s get you back on track."
  >
    <a class="wu-btn wu-btn--primary" [routerLink]="home">Back to safety</a>
  </wu-state-page>`,
})
export class NotFound {
  private readonly auth = inject(AuthStore);
  protected readonly home = this.auth.isAuthenticated()
    ? roleHome(this.auth.role())
    : '/auth/sign-in';
}
