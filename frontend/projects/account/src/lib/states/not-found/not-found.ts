import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-007
@Component({
  selector: 'w-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css',
})
export class NotFound {
  private readonly auth = inject(AuthStore);
  protected readonly home = this.auth.isAuthenticated()
    ? roleHome(this.auth.role())
    : '/auth/sign-in';
}
