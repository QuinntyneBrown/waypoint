import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-007
@Component({
  selector: 'w-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  template: `<w-state-page
    icon="compass"
    tone="info"
    eyebrow="Error 404"
    title="Page not found"
    text="The page you’re looking for moved or never existed. Let’s get you back on track."
  >
    <a w-button variant="primary" [routerLink]="home">Back to safety</a>
  </w-state-page>`,
})
export class NotFound {
  private readonly auth = inject(AuthStore);
  protected readonly home = this.auth.isAuthenticated()
    ? roleHome(this.auth.role())
    : '/auth/sign-in';
}
