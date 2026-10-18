import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-reset-success',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  template: `<w-state-page
    icon="check-circle"
    tone="success"
    eyebrow="All set"
    title="Your password was updated"
    text="For your security we signed out your other sessions. Sign in with your new password."
  >
    <a w-button variant="primary" routerLink="/auth/sign-in">Sign in</a>
  </w-state-page>`,
})
export class ResetSuccess {}
