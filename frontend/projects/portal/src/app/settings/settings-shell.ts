import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';
import { shellRole } from '../shell-role';

/** Shared layout for the settings pages: app shell + settings tab nav + content. */
@Component({
  selector: 'wu-settings-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, RouterLink, RouterLinkActive],
  template: `<wu-shell
    [role]="role()"
    active="profile"
    [userName]="userName()"
    (signOut)="signOut()"
  >
    <div class="wu-container wu-container--narrow wu-page">
      <div class="wu-page__head">
        <div class="wu-page__titles">
          <div class="wu-eyebrow">Settings</div>
          <h1 class="wu-page__title">{{ heading() }}</h1>
        </div>
      </div>
      <div class="wu-tabs wu-mb-6" role="tablist">
        <a class="wu-tab" routerLink="/settings/profile" routerLinkActive="is-active">Profile</a>
        <a class="wu-tab" routerLink="/settings/notifications" routerLinkActive="is-active">
          Notifications
        </a>
        <a class="wu-tab" routerLink="/settings/display" routerLinkActive="is-active">Display</a>
      </div>
      <ng-content />
    </div>
  </wu-shell>`,
})
export class SettingsShell {
  readonly heading = input('Settings');
  private readonly auth = inject(AuthStore);
  protected readonly role = computed(() => shellRole(this.auth.role()));
  protected readonly userName = computed(() => this.auth.user()?.displayName ?? '');
  protected signOut(): void {
    this.auth.signOut();
  }
}
