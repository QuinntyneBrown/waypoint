import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'wu-public-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `<section class="wu-container wu-section" style="max-width: 560px; text-align: center">
    <div class="wu-eyebrow">Error 404</div>
    <h1>We couldn’t find that page</h1>
    <p class="wu-text-lg wu-muted">The link may be broken or the page may have moved.</p>
    <div class="wu-cluster wu-mt-6" style="justify-content: center">
      <a class="wu-btn wu-btn--primary" routerLink="/">Back to home</a>
      <a class="wu-btn wu-btn--ghost" routerLink="/apply">Apply</a>
    </div>
  </section>`,
})
export class NotFound {}
