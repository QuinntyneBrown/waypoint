import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Button, StatePage } from '@waypoint/components';
import { FormDraftService } from '@waypoint/core';

// Traces to: L2-PLATFORM-010
@Component({
  selector: 'ph-session-expired',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatePage, RouterLink, Button],
  templateUrl: './session-expired.html',
  styleUrl: './session-expired.css',
})
export class SessionExpired {
  private readonly route = inject(ActivatedRoute);
  private readonly drafts = inject(FormDraftService);
  protected readonly returnUrl =
    this.route.snapshot.queryParamMap.get('returnUrl') ?? this.drafts.draftUrl();
}
