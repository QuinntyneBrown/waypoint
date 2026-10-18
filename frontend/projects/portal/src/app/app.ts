import { DOCUMENT } from '@angular/common';
import { Component, effect, EnvironmentInjector, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DisplayPreferencesService as DisplayPreferencesApiService } from '@waypoint/api';
import { SessionStore } from '@waypoint/auth';
import { ToastHost } from '@waypoint/components';
import { DisplayPreferencesService, IdleSessionService } from '@waypoint/core';

const NON_CRITICAL_RUNTIME_DELAY_MS = 1_500;

@Component({
  selector: 'w-root',
  imports: [RouterOutlet, ToastHost],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly session = inject(SessionStore);
  private readonly displayApi = inject(DisplayPreferencesApiService);
  private readonly applier = inject(DisplayPreferencesService);
  private readonly environment = inject(EnvironmentInjector);
  private readonly window = inject(DOCUMENT).defaultView;
  private appliedFor: string | null = null;
  private runtimeTimer: ReturnType<typeof setTimeout> | null = null;
  private realtimeStarted = false;
  private attendanceQueueStarted = false;

  constructor() {
    // Enforce shared-device inactivity expiry and draft-preserving recovery app-wide.
    void inject(IdleSessionService);
    // Re-apply saved display & accessibility preferences app-wide once a user is
    // authenticated (and again when a different user signs in).
    effect(() => {
      const user = this.session.user();
      if (user && this.appliedFor !== user.userId) {
        this.appliedFor = user.userId;
        this.displayApi.get().subscribe({ next: (p) => this.applier.apply(p), error: () => {} });
      }
      if (user) this.scheduleNonCriticalRuntime();
    });
  }

  /**
   * Realtime and offline replay remain app-wide, but neither is required to
   * render or operate the first route. Loading them shortly after bootstrap
   * keeps SignalR and attendance code off the critical navigation path.
   */
  private scheduleNonCriticalRuntime(): void {
    if (!this.window || this.runtimeTimer) return;
    const needsRealtime = !this.realtimeStarted;
    const role = this.session.user()?.role;
    const needsAttendance =
      !this.attendanceQueueStarted && (role === 'Mentor' || role === 'Volunteer');
    if (!needsRealtime && !needsAttendance) return;

    this.runtimeTimer = this.window.setTimeout(() => {
      this.runtimeTimer = null;
      const currentUser = this.session.user();
      if (!currentUser) return;

      if (!this.realtimeStarted) {
        this.realtimeStarted = true;
        void import('@waypoint/core/realtime')
          .then(({ RealtimeService }) => this.environment.get(RealtimeService))
          .catch(() => (this.realtimeStarted = false));
      }

      if (
        !this.attendanceQueueStarted &&
        (currentUser.role === 'Mentor' || currentUser.role === 'Volunteer')
      ) {
        this.attendanceQueueStarted = true;
        void import('@waypoint/core/attendance')
          .then(({ AttendanceQueueService }) => this.environment.get(AttendanceQueueService))
          .catch(() => (this.attendanceQueueStarted = false));
      }
    }, NON_CRITICAL_RUNTIME_DELAY_MS);
  }
}
