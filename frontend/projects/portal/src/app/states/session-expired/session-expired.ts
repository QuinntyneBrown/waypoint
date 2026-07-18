import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'wu-session-expired',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="clock"
    tone="warning"
    eyebrow="Session expired"
    title="You’ve been signed out"
    text="For your security we signed you out after a period of inactivity. Any work in progress has been saved — sign in to pick up where you left off."
  >
    <a class="wu-btn wu-btn--primary" routerLink="/auth/sign-in">Sign in again</a>
  </wu-state-page>`,
})
export class SessionExpired {}
