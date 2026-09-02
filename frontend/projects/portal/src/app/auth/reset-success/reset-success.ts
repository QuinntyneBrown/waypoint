import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WStatePage } from '@w/ui';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-reset-success',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage, RouterLink],
  template: `<w-state-page
    icon="check-circle"
    tone="success"
    eyebrow="All set"
    title="Your password was updated"
    text="For your security we signed out your other sessions. Sign in with your new password."
  >
    <a class="w-btn w-btn--primary" routerLink="/auth/sign-in">Sign in</a>
  </w-state-page>`,
})
export class ResetSuccess {}
