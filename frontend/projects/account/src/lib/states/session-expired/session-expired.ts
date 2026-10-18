import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button, StatePage } from '@waypoint/components';
import { FormDraftService } from '@waypoint/core';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'w-session-expired',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  template: `<w-state-page
    icon="clock"
    tone="warning"
    eyebrow="Session expired"
    title="You’ve been signed out"
    text="For your security we signed you out after a period of inactivity. Any work in progress has been saved — sign in to pick up where you left off."
  >
    <a
      w-button
      variant="primary"
      routerLink="/auth/sign-in"
      [queryParams]="returnUrl ? { returnUrl } : null"
      >Sign in again</a
    >
  </w-state-page>`,
})
export class SessionExpired {
  private readonly route = inject(ActivatedRoute);
  private readonly drafts = inject(FormDraftService);
  protected readonly returnUrl =
    this.route.snapshot.queryParamMap.get('returnUrl') ?? this.drafts.draftUrl();
}
