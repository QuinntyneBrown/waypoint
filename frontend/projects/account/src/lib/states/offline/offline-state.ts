import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Button, StatePage } from '@waypoint/components';

// Traces to: L2-PLATFORM-012
@Component({
  selector: 'w-offline-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, Button],
  template: `<w-state-page
    icon="wifi-off"
    tone="warning"
    eyebrow="You’re offline"
    title="No connection right now"
    text="Check your internet connection. Anything already loaded stays available, and we’ll reconnect automatically."
  >
    <button w-button variant="primary" type="button" (click)="retry()">Retry</button>
  </w-state-page>`,
})
export class OfflineState {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
