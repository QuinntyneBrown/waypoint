import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-012
@Component({
  selector: 'w-offline-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, Button],
  templateUrl: './offline-state.html',
  styleUrl: './offline-state.css',
})
export class OfflineState {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
