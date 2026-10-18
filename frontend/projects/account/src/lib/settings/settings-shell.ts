import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore } from '@waypoint/auth';
import { Shell, Tabs, Tab } from '@waypoint/components';
import { shellRole } from '@waypoint/core';

/** Shared layout for the settings pages: app shell + settings tab nav + content. */
@Component({
  selector: 'w-settings-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Shell, RouterLink, RouterLinkActive, Tabs, Tab],
  templateUrl: './settings-shell.html',
  styleUrl: './settings-shell.css',
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
