import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

@Component({
  selector: 'wu-youth-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell],
  template: `<wu-shell
    role="youth"
    active="home"
    [userName]="auth.user()?.displayName ?? ''"
    (signOut)="auth.signOut()"
  >
    <div class="wu-container wu-container--narrow wu-page">
      <div class="wu-page__head">
        <div class="wu-page__titles">
          <div class="wu-eyebrow">Home</div>
          <h1 class="wu-page__title">Welcome, {{ firstName() }}</h1>
          <p class="wu-page__sub">You’re signed in. Your journey picks up here.</p>
        </div>
      </div>

      <div class="wu-card wu-card--greige">
        <div class="wu-card__body">
          <div class="wu-scripture">
            “For even the Son of Man did not come to be served, but to serve.”
            <span class="wu-scripture__ref">Mark 10:45</span>
          </div>
        </div>
      </div>
    </div>
  </wu-shell>`,
})
export class YouthHome {
  protected readonly auth = inject(AuthStore);
  protected readonly firstName = computed(
    () => (this.auth.user()?.displayName ?? '').split(/\s+/)[0] || 'friend',
  );
}
