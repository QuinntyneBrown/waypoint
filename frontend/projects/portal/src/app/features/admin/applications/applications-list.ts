import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ApplicationsApi, ApplicationSummary } from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuShell } from '@wu/ui';

// Traces to: L2-INTAKE-002
@Component({
  selector: 'wu-applications-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, DatePipe],
  templateUrl: './applications-list.html',
})
export class ApplicationsList {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(ApplicationsApi);

  /** undefined = still loading; array (possibly empty) = loaded. */
  protected readonly applications = signal<ApplicationSummary[] | undefined>(undefined);
  protected readonly loading = computed(() => this.applications() === undefined);

  constructor() {
    this.api.list().subscribe({
      next: (items) => this.applications.set(items),
      error: () => this.applications.set([]),
    });
  }
}
