import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'w-public-not-found',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `<section class="w-container w-section" style="max-width: 560px; text-align: center">
    <div class="w-eyebrow">Error 404</div>
    <h1>We couldn’t find that page</h1>
    <p class="w-text-lg w-muted">The link may be broken or the page may have moved.</p>
    <div class="w-cluster w-mt-6" style="justify-content: center">
      <a class="w-btn w-btn--primary" routerLink="/">Back to home</a>
      <a class="w-btn w-btn--ghost" routerLink="/apply">Apply</a>
    </div>
  </section>`,
})
export class NotFound {}
