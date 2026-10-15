import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@waypoint/auth';
import { WStatePage } from '@waypoint/components';
import { FormDraftService } from '@waypoint/core';

// Traces to: L2-PLATFORM-008
@Component({
  selector: 'w-server-error',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink],
  template: `<w-state-page
    icon="alert-triangle"
    tone="error"
    eyebrow="Error 500"
    title="Something went wrong"
    text="We hit a snag on our end — nothing you did caused it. Please try again in a moment."
  >
    <button class="w-btn w-btn--primary" type="button" (click)="retry()">Try again</button>
    <a class="w-btn w-btn--ghost" [routerLink]="home">Back to home</a>
  </w-state-page>`,
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
