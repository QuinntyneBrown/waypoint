import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { WuStatePage } from '@wu/ui';

// Traces to: L2-PLATFORM-009
@Component({
  selector: 'wu-maintenance',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuStatePage],
  template: `<wu-state-page
    icon="settings"
    tone="info"
    eyebrow="Scheduled maintenance"
    title="We’ll be right back"
    text="Word Up is down for a short bit of maintenance. Nothing you’ve done is lost — please check again shortly."
  >
    <button class="wu-btn wu-btn--primary" type="button" (click)="retry()">Try again</button>
  </wu-state-page>`,
})
export class Maintenance {
  private readonly doc = inject(DOCUMENT);
  protected retry(): void {
    this.doc.defaultView?.location.reload();
  }
}
