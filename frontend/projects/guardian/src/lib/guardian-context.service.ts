import { computed, inject, Injectable, signal } from '@angular/core';
import { ApiError, GuardianApi, GuardianChildProgress, GuardianChildSummary } from '@waypoint/api';

// Shared linked-child state across Guardian home/progress routes.
@Injectable({ providedIn: 'root' })
export class GuardianContextService {
  private readonly api = inject(GuardianApi);
  private loaded = false;

  readonly children = signal<GuardianChildSummary[] | undefined>(undefined);
  readonly selectedChildId = signal<string | null>(null);
  readonly progress = signal<GuardianChildProgress | null | undefined>(undefined);
  readonly error = signal<string | null>(null);
  readonly loading = computed(() => this.children() === undefined || this.progress() === undefined);

  load(): void {
    if (this.loaded) return;
    this.loaded = true;
    this.api.children().subscribe({
      next: (children) => {
        this.children.set(children);
        if (children.length === 0) {
          this.progress.set(null);
          return;
        }
        this.select(children[0].childId);
      },
      error: (error: unknown) => {
        this.children.set([]);
        this.progress.set(null);
        this.error.set(ApiError.from(error).title);
      },
    });
  }

  select(childId: string): void {
    this.selectedChildId.set(childId);
    this.progress.set(undefined);
    this.error.set(null);
    this.api.progress(childId).subscribe({
      next: (progress) => this.progress.set(progress),
      error: (error: unknown) => {
        this.progress.set(null);
        this.error.set(ApiError.from(error).title);
      },
    });
  }
}
