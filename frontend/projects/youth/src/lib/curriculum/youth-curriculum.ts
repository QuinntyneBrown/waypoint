import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiError, LearningService, YouthCurriculum } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

// Traces to: L2-LEARN-001
@Component({
  selector: 'w-youth-curriculum',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DecimalPipe],
  templateUrl: './youth-curriculum.html',
})
export class YouthCurriculumPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(LearningService);

  protected readonly curriculum = signal<YouthCurriculum | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly switching = signal(false);

  constructor() {
    this.load();
  }

  protected switchTrack(trackId: string): void {
    if (this.switching() || trackId === this.curriculum()?.selectedTrackId) return;
    this.load(trackId);
  }

  private load(trackId?: string): void {
    this.switching.set(true);
    this.errorMessage.set(null);
    this.api.curriculum(trackId).subscribe({
      next: (curriculum) => {
        this.curriculum.set(curriculum);
        this.switching.set(false);
      },
      error: (error: unknown) => {
        this.switching.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }
}
