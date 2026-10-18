import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-006
@Component({
  selector: 'w-forbidden',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './forbidden.html',
  styleUrl: './forbidden.css',
})
export class Forbidden {
  private readonly auth = inject(AuthStore);
  protected readonly home = roleHome(this.auth.role());
  protected switchAccount(): void {
    this.auth.signOut();
  }
}
