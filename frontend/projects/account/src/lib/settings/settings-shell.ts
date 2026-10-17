import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import { WShell, WTabs, WTab } from '@waypoint/components';
import { shellRole } from '@waypoint/core';

/** Shared layout for the settings pages: app shell + settings tab nav + content. */
@Component({
  selector: 'w-settings-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, RouterLinkActive, WTabs, WTab],
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
      <nav w-tabs class="w-mb-6" aria-label="Settings">
        <a
          w-tab
          routerLink="/settings/profile"
          routerLinkActive
          #profile="routerLinkActive"
          [active]="profile.isActive"
          >Profile</a
        >
        <a
          w-tab
          routerLink="/settings/notifications"
          routerLinkActive
          #notifications="routerLinkActive"
          [active]="notifications.isActive"
        >
          Notifications
        </a>
        <a
          w-tab
          routerLink="/settings/display"
          routerLinkActive
          #display="routerLinkActive"
          [active]="display.isActive"
          >Display</a
        >
        <a
          w-tab
          routerLink="/settings/privacy"
          routerLinkActive
          #privacy="routerLinkActive"
          [active]="privacy.isActive"
          >Privacy</a
        >
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
