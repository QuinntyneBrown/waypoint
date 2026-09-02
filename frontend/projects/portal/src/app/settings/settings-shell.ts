import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '@w/auth';
import { WShell } from '@w/ui';
import { shellRole } from '../shell-role';

/** Shared layout for the settings pages: app shell + settings tab nav + content. */
@Component({
  selector: 'w-settings-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, RouterLinkActive],
  template: `<w-shell
    [role]="role()"
    active="profile"
    [userName]="userName()"
    (signOut)="signOut()"
  >
    <div class="w-container w-container--narrow w-page">
      <div class="w-page__head">
        <div class="w-page__titles">
          <div class="w-eyebrow">Settings</div>
          <h1 class="w-page__title">{{ heading() }}</h1>
        </div>
      </div>
      <nav class="w-tabs w-mb-6" aria-label="Settings">
        <a class="w-tab" routerLink="/settings/profile" routerLinkActive="is-active">Profile</a>
        <a class="w-tab" routerLink="/settings/notifications" routerLinkActive="is-active">
          Notifications
        </a>
        <a class="w-tab" routerLink="/settings/display" routerLinkActive="is-active">Display</a>
        <a class="w-tab" routerLink="/settings/privacy" routerLinkActive="is-active">Privacy</a>
      </nav>
      <ng-content />
    </div>
  </w-shell>`,
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
