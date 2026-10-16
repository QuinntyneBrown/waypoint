import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiError, MentorService, MentorYouthDetail } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';

type DetailTab =
  'profile' | 'progress' | 'attendance' | 'check-ins' | 'submissions' | 'competencies';

// Traces to: L2-MENTOR-002, L2-AUTH-010
@Component({
  selector: 'w-mentor-youth-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, RouterLink, DatePipe],
  templateUrl: './mentor-youth-detail.html',
  styles: `
    .detail-tabs {
      display: flex;
      gap: var(--w-s2);
      overflow-x: auto;
      padding-block: var(--w-s2);
    }
    .detail-tab {
      min-height: 44px;
      flex: none;
    }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s5);
    }
    .detail-list {
      margin: 0;
      padding: 0;
      list-style: none;
    }
    .detail-row {
      padding: var(--w-s4) 0;
      border-bottom: 1px solid var(--w-border);
      overflow-wrap: anywhere;
    }
    .detail-row:last-child {
      border-bottom: 0;
    }
    @media (max-width: 40rem) {
      .detail-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class MentorYouthDetailPage {
  protected readonly auth = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(MentorService);

  protected readonly detail = signal<MentorYouthDetail | null>(null);
  protected readonly activeTab = signal<DetailTab>('profile');
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly tabs: readonly { key: DetailTab; label: string }[] = [
    { key: 'profile', label: 'Profile' },
    { key: 'progress', label: 'Progress' },
    { key: 'attendance', label: 'Attendance' },
    { key: 'check-ins', label: 'Check-ins' },
    { key: 'submissions', label: 'Submissions' },
    { key: 'competencies', label: 'Competencies' },
  ];

  constructor() {
    const youthId = this.route.snapshot.paramMap.get('id') ?? '';
    this.api.youth(youthId).subscribe({
      next: (detail) => this.detail.set(detail),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  protected selectTab(tab: DetailTab): void {
    this.activeTab.set(tab);
  }

  protected percent(value: number): number {
    return Math.round(value * 100);
  }
}
