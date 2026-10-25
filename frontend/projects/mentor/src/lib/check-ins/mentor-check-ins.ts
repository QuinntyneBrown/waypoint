import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiError, MentorService, MentorCheckIn, MentorRosterYouth } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  Button,
  FormField,
  Help,
  Icon,
  Label,
  Select,
  Shell,
  Textarea,
  TextField,
  Checkbox,
  CheckboxBox,
  Alert,
  Card,
  CardHeader,
  CardTitle,
  CardBody,
  Badge,
  EmptyState,
  EmptyStateIcon,
  EmptyStateTitle,
  EmptyStateText,
} from '@waypoint/components';

type CheckInType = '1:1' | 'call' | 'session';

// Traces to: L2-MENTOR-006, L2-LEARN-010, L2-GUARDIAN-001
@Component({
  selector: 'ph-mentor-check-ins',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    Shell,
    Icon,
    DatePipe,
    Button,
    FormField,
    Label,
    Help,
    TextField,
    Textarea,
    Select,
    Checkbox,
    CheckboxBox,
    Alert,
    Card,
    CardHeader,
    CardTitle,
    CardBody,
    Badge,
    EmptyState,
    EmptyStateIcon,
    EmptyStateTitle,
    EmptyStateText,
  ],
  templateUrl: './mentor-check-ins.html',
  styleUrl: './mentor-check-ins.css',
})
export class MentorCheckInsPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorService);
  private readonly route = inject(ActivatedRoute);

  protected readonly youth = signal<MentorRosterYouth[]>([]);
  protected readonly history = signal<MentorCheckIn[] | null>(null);
  protected readonly selectedYouthId = signal(
    this.route.snapshot.queryParamMap.get('youthId') ?? '',
  );
  protected readonly date = signal(new Date().toISOString().slice(0, 10));
  protected readonly type = signal<CheckInType>('1:1');
  protected readonly engagement = signal('');
  protected readonly goalsDiscussed = signal('');
  protected readonly prayerRequests = signal('');
  protected readonly actionItems = signal('');
  protected readonly nextFollowUp = signal('');
  protected readonly share = signal(false);
  protected readonly guardianInformed = signal(false);
  protected readonly filterYouthId = signal(this.route.snapshot.queryParamMap.get('youthId') ?? '');
  protected readonly filterType = signal('');
  protected readonly saving = signal(false);
  protected readonly statusMessage = signal<string | null>(null);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.api.roster('all').subscribe({
      next: (youth) => {
        this.youth.set(youth);
        if (!this.selectedYouthId() && youth.length === 1) this.selectedYouthId.set(youth[0].id);
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
    this.loadHistory();
  }

  protected save(): void {
    if (!this.selectedYouthId()) {
      this.errorMessage.set('Choose a youth before saving the check-in.');
      return;
    }
    if (!this.date()) {
      this.errorMessage.set('Choose a check-in date.');
      return;
    }
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .logCheckIn({
        youthId: this.selectedYouthId(),
        date: `${this.date()}T12:00:00.000Z`,
        type: this.type(),
        engagement: this.optional(this.engagement()),
        goalsDiscussed: this.optional(this.goalsDiscussed()),
        prayerRequests: this.optional(this.prayerRequests()),
        actionItems: this.optional(this.actionItems()),
        nextFollowUp: this.nextFollowUp() ? `${this.nextFollowUp()}T12:00:00.000Z` : null,
        shareWithYouthAndGuardian: this.share(),
        guardianInformed: this.guardianInformed(),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.statusMessage.set('Check-in saved.');
          this.filterYouthId.set(this.selectedYouthId());
          this.resetNotes();
          this.loadHistory();
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.errorMessage.set(ApiError.from(error).title);
        },
      });
  }

  protected applyFilters(): void {
    this.loadHistory();
  }

  private loadHistory(): void {
    this.api.checkIns(this.filterYouthId(), this.filterType()).subscribe({
      next: (history) => this.history.set(history),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private optional(value: string): string | null {
    return value.trim() || null;
  }

  private resetNotes(): void {
    this.engagement.set('');
    this.goalsDiscussed.set('');
    this.prayerRequests.set('');
    this.actionItems.set('');
    this.nextFollowUp.set('');
    this.share.set(false);
    this.guardianInformed.set(false);
  }
}
