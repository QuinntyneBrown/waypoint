import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  ApiError,
  CohortMentorAssignment,
  MentorAssignmentOverview,
  MentorAssignmentsService,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  FormField,
  Icon,
  Label,
  Select,
  Shell,
  Alert,
  AlertTitle,
  Card,
  CardBody,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
  StatLabel,
  StatValue,
} from '@waypoint/components';
import { finalize } from 'rxjs';

// Traces to: L2-ADMIN-005, L2-REPORT-001, L2-NFR-009, L2-NFR-010
@Component({
  selector: 'w-mentor-assignments',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    Button,
    FormField,
    Label,
    Select,
    Alert,
    AlertTitle,
    Card,
    CardBody,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
    StatLabel,
    StatValue,
  ],
  templateUrl: './mentor-assignments.html',
  styleUrl: './mentor-assignments.css',
})
export class MentorAssignmentsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorAssignmentsService);

  protected readonly overview = signal<MentorAssignmentOverview | undefined>(undefined);
  protected readonly youthId = signal('');
  protected readonly mentorId = signal('');
  protected readonly cohortId = signal('');
  protected readonly leadMentorId = signal('');
  protected readonly supportingMentorIds = signal<ReadonlySet<string>>(new Set());
  protected readonly saving = signal<'youth' | 'cohort' | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly outcome = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected selectYouth(id: string): void {
    this.youthId.set(id);
    const youth = this.overview()?.youth.find((item) => item.id === id);
    this.mentorId.set(youth?.mentorId ?? this.overview()?.mentors[0]?.mentorId ?? '');
  }

  protected selectMentor(id: string): void {
    this.mentorId.set(id);
  }

  protected assignMentor(): void {
    const youthId = this.youthId();
    const mentorId = this.mentorId();
    if (!youthId || !mentorId || this.saving()) return;
    this.saving.set('youth');
    this.errorMessage.set(null);
    this.outcome.set(null);
    this.api
      .assignYouth(youthId, mentorId)
      .pipe(finalize(() => this.saving.set(null)))
      .subscribe({
        next: () => {
          this.outcome.set('Mentor assignment saved. The mentor roster and load are updated.');
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected selectCohort(id: string): void {
    this.cohortId.set(id);
    this.syncCohortTeam();
  }

  protected selectLeadMentor(id: string): void {
    this.leadMentorId.set(id);
    const supporting = new Set(this.supportingMentorIds());
    supporting.delete(id);
    this.supportingMentorIds.set(supporting);
  }

  protected toggleSupportingMentor(id: string, checked: boolean): void {
    const supporting = new Set(this.supportingMentorIds());
    if (checked) supporting.add(id);
    else supporting.delete(id);
    this.supportingMentorIds.set(supporting);
  }

  protected isSupporting(id: string): boolean {
    return this.supportingMentorIds().has(id);
  }

  protected saveCohortTeam(): void {
    const cohortId = this.cohortId();
    const leadMentorId = this.leadMentorId();
    if (!cohortId || !leadMentorId || this.saving()) return;
    this.saving.set('cohort');
    this.errorMessage.set(null);
    this.outcome.set(null);
    this.api
      .setCohortMentors(cohortId, leadMentorId, [...this.supportingMentorIds()])
      .pipe(finalize(() => this.saving.set(null)))
      .subscribe({
        next: () => {
          this.outcome.set('Cohort mentor team saved.');
          this.load(false);
        },
        error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
      });
  }

  protected retry(): void {
    this.load();
  }

  protected youthLabel(count: number): string {
    return `${count} ${count === 1 ? 'youth' : 'youth'}`;
  }

  private load(showLoading = true): void {
    if (showLoading) this.overview.set(undefined);
    this.errorMessage.set(null);
    this.api.overview().subscribe({
      next: (overview) => {
        this.overview.set(overview);
        const youthId = overview.youth.some((item) => item.id === this.youthId())
          ? this.youthId()
          : (overview.youth[0]?.id ?? '');
        this.youthId.set(youthId);
        const selectedYouth = overview.youth.find((item) => item.id === youthId);
        if (!overview.mentors.some((item) => item.mentorId === this.mentorId())) {
          this.mentorId.set(selectedYouth?.mentorId ?? overview.mentors[0]?.mentorId ?? '');
        }
        if (!overview.cohorts.some((item) => item.id === this.cohortId())) {
          this.cohortId.set(overview.cohorts[0]?.id ?? '');
        }
        this.syncCohortTeam();
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private syncCohortTeam(): void {
    const cohort = this.selectedCohort();
    this.leadMentorId.set(cohort?.leadMentorId ?? this.overview()?.mentors[0]?.mentorId ?? '');
    this.supportingMentorIds.set(
      new Set(cohort?.supportingMentors.map((mentor) => mentor.id) ?? []),
    );
  }

  private selectedCohort(): CohortMentorAssignment | undefined {
    return this.overview()?.cohorts.find((item) => item.id === this.cohortId());
  }
}
