import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-012
@Component({
  selector: 'wu-offline-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage],
  template: `<wu-state-page
    icon="wifi-off"
    tone="warning"
    eyebrow="You’re offline"
    title="No connection right now"
    text="Check your internet connection. Anything already loaded stays available, and we’ll reconnect automatically."
  >
    <button class="wu-btn wu-btn--primary" type="button" (click)="retry()">Retry</button>
  </wu-state-page>`,
})
export class OfflineState {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
