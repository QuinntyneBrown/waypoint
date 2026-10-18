import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-AUTH-005
@Component({
  selector: 'w-reset-success',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './reset-success.html',
  styleUrl: './reset-success.css',
})
export class ResetSuccess {}
