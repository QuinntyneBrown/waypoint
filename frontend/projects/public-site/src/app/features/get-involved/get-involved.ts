import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'wu-get-involved',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `<section class="wu-container wu-section" style="max-width: 720px">
    <div class="wu-eyebrow">Get involved</div>
    <h1>Help raise up the next generation</h1>
    <p class="wu-text-lg wu-muted">
      There’s a place for you — as a mentor, a volunteer, or a monthly giver.
    </p>

    <div class="wu-grid wu-grid--2 wu-mt-6">
      <div class="wu-card">
        <div class="wu-card__body">
          <h2 class="wu-card__title">Volunteer</h2>
          <p class="wu-muted">
            Walk alongside young people as a mentor or helper. Because you’ll work with youth, every
            volunteer completes a screening and reference check before starting.
          </p>
          <a class="wu-btn wu-btn--secondary" routerLink="/contact">Talk to our team</a>
        </div>
      </div>
      <div class="wu-card">
        <div class="wu-card__body">
          <h2 class="wu-card__title">Give monthly</h2>
          <p class="wu-muted">
            A recurring gift keeps cohorts running and mentors resourced all year long.
          </p>
          <a class="wu-btn wu-btn--primary" routerLink="/donate">Donate</a>
        </div>
      </div>
    </div>
  </section>`,
})
export class GetInvolved {}
