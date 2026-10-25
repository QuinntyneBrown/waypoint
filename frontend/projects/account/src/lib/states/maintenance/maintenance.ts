import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-009
@Component({
  selector: 'ph-maintenance',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, Button],
  templateUrl: './maintenance.html',
  styleUrl: './maintenance.css',
})
export class Maintenance {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
