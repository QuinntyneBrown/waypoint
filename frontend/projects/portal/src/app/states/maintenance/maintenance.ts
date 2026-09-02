import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { WStatePage } from '@w/ui';

// Traces to: L2-PLATFORM-009
@Component({
  selector: 'w-maintenance',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WStatePage],
  template: `<w-state-page
    icon="settings"
    tone="info"
    eyebrow="Scheduled maintenance"
    title="We’ll be right back"
    text="Waypoint is down for a short bit of maintenance. Nothing you’ve done is lost — please check again shortly."
  >
    <button class="w-btn w-btn--primary" type="button" (click)="retry()">Try again</button>
  </w-state-page>`,
})
export class Maintenance {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
