import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { Button, StatePage } from '@waypoint/components';
import { FormDraftService } from '@waypoint/core';

// Traces to: L2-PLATFORM-008
@Component({
  selector: 'w-server-error',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './server-error.html',
  styleUrl: './server-error.css',
})
export class ServerError {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly drafts = inject(FormDraftService);
  protected readonly home = this.auth.isAuthenticated()
    ? roleHome(this.auth.role())
    : '/auth/sign-in';
  protected retry(): void {
    const target = this.drafts.draftUrl() ?? this.home;
    void this.router.navigateByUrl(target).then(() => {
      setTimeout(() => this.drafts.restoreAndClear());
    });
  }
}
