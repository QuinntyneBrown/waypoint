import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthStore, roleHome } from '@wu/auth';
import { WuStatePage } from '@wu/ui';
import { FormDraftService } from '../../form-draft.service';

// Traces to: L2-PLATFORM-008
@Component({
  selector: 'wu-server-error',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="alert-triangle"
    tone="error"
    eyebrow="Error 500"
    title="Something went wrong"
    text="We hit a snag on our end — nothing you did caused it. Please try again in a moment."
  >
    <button class="wu-btn wu-btn--primary" type="button" (click)="retry()">Try again</button>
    <a class="wu-btn wu-btn--ghost" [routerLink]="home">Back to home</a>
  </wu-state-page>`,
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
