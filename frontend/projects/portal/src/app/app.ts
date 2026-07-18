import { Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DisplayPreferencesApi } from '@wu/api';
import { SessionStore } from '@wu/auth';
import { WuToastHost } from '@wu/ui';
import { DisplayPreferencesService } from './settings/display-preferences.service';

@Component({
  selector: 'wu-root',
  imports: [RouterOutlet, WuToastHost],
  template: `<router-outlet /> <wu-toast-host />`,
})
export class App {
  private readonly session = inject(SessionStore);
  private readonly displayApi = inject(DisplayPreferencesApi);
  private readonly applier = inject(DisplayPreferencesService);
  private appliedFor: string | null = null;

  constructor() {
    // Re-apply saved display & accessibility preferences app-wide once a user is
    // authenticated (and again when a different user signs in).
    effect(() => {
      const user = this.session.user();
      if (user && this.appliedFor !== user.userId) {
        this.appliedFor = user.userId;
        this.displayApi.get().subscribe({ next: (p) => this.applier.apply(p), error: () => {} });
      }
    });
  }
}
