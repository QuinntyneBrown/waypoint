import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'wu-contact',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `<section class="wu-container wu-section" style="max-width: 640px">
    <div class="wu-eyebrow">Contact</div>
    <h1>Get in touch</h1>
    <p class="wu-text-lg wu-muted">
      Questions about the program, volunteering, or partnering with us? We’d love to hear from you.
    </p>

    <div class="wu-card wu-mt-6">
      <div class="wu-card__body wu-stack">
        <div>
          <div class="wu-eyebrow">Email</div>
          <p><a href="mailto:hello@wordup.example">hello&#64;wordup.example</a></p>
        </div>
        <div>
          <div class="wu-eyebrow">Where we are</div>
          <p>Toronto, Ontario</p>
        </div>
        <div>
          <div class="wu-eyebrow">Program director</div>
          <p>Shawn Peters</p>
        </div>
      </div>
    </div>

    <div class="wu-cluster wu-mt-6">
      <a class="wu-btn wu-btn--primary" routerLink="/apply">Apply to a cohort</a>
      <a class="wu-btn wu-btn--ghost" routerLink="/get-involved">Get involved</a>
    </div>
  </section>`,
})
export class Contact {}
