import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ApiError,
  CompetencyLevel,
  MentorService,
  MentorCompetencyItem,
  MentorCompetencySummary,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WIcon,
  WLabel,
  WLink,
  WSelect,
  WShell,
  WTextarea,
  WAlert,
} from '@waypoint/components';

interface CompetencyDraft {
  level: CompetencyLevel;
  notes: string;
}

// Traces to: L2-MENTOR-005, L2-LEARN-010
@Component({
  selector: 'w-mentor-competencies',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WIcon,
    RouterLink,
    DatePipe,
    WButton,
    WLink,
    WFormField,
    WLabel,
    WTextarea,
    WSelect,
    WAlert,
  ],
  templateUrl: './mentor-competencies.html',
  styles: `
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: var(--w-s4);
    }
    .competency-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--w-s5);
    }
    .assessment-record {
      min-height: 2.75rem;
    }
    .save-button {
      min-height: 44px;
    }
    @media (max-width: 48rem) {
      .summary-grid,
      .competency-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class MentorCompetenciesPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);
  private readonly route = inject(ActivatedRoute);

  protected readonly levels: { value: CompetencyLevel; label: string }[] = [
    { value: 'NotStarted', label: 'Not started' },
    { value: 'Developing', label: 'Developing' },
    { value: 'Proficient', label: 'Proficient' },
    { value: 'Mastered', label: 'Mastered' },
  ];
  protected readonly youthId = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly summary = signal<MentorCompetencySummary | null>(null);
  protected readonly drafts = signal<Record<string, CompetencyDraft>>({});
  protected readonly savingId = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected draft(item: MentorCompetencyItem): CompetencyDraft {
    return this.drafts()[item.competencyId] ?? { level: item.level, notes: item.notes ?? '' };
  }

  protected levelLabel(level: CompetencyLevel): string {
    return this.levels.find((option) => option.value === level)?.label ?? level;
  }

  protected setLevel(item: MentorCompetencyItem, level: CompetencyLevel): void {
    this.updateDraft(item, { level });
  }

  protected setNotes(item: MentorCompetencyItem, notes: string): void {
    this.updateDraft(item, { notes });
  }

  protected save(item: MentorCompetencyItem): void {
    if (this.savingId()) return;
    const draft = this.draft(item);
    this.savingId.set(item.competencyId);
    this.errorMessage.set(null);
    this.api
      .setCompetencyLevel(this.youthId, {
        competencyId: item.competencyId,
        level: draft.level,
        notes: draft.notes.trim() || null,
      })
      .subscribe({
        next: () => {
          this.savingId.set(null);
          this.statusMessage.set(`Saved ${item.name}.`);
          this.load();
        },
        error: (error: unknown) => {
          this.savingId.set(null);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }

  private updateDraft(item: MentorCompetencyItem, patch: Partial<CompetencyDraft>): void {
    const current = this.draft(item);
    this.drafts.update((drafts) => ({
      ...drafts,
      [item.competencyId]: { ...current, ...patch },
    }));
    this.statusMessage.set(null);
  }

  private load(): void {
    this.api.competencies(this.youthId).subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.drafts.set(
          Object.fromEntries(
            summary.items.map((item) => [
              item.competencyId,
              { level: item.level, notes: item.notes ?? '' },
            ]),
          ),
        );
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
