import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-AUTH-005
@Component({
  selector: 'ph-reset-link-invalid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './reset-link-invalid.html',
  styleUrl: './reset-link-invalid.css',
})
export class ResetLinkInvalid {}
