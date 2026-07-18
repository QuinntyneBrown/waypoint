import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-AUTH-005
@Component({
  selector: 'wu-reset-success',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage, RouterLink],
  template: `<wu-state-page
    icon="check-circle"
    tone="success"
    eyebrow="All set"
    title="Your password was updated"
    text="For your security we signed out your other sessions. Sign in with your new password."
  >
    <a class="wu-btn wu-btn--primary" routerLink="/auth/sign-in">Sign in</a>
  </wu-state-page>`,
})
export class ResetSuccess {}
