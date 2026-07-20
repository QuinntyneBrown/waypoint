import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  ApiError,
  AssignableWorkItem,
  AssignWorkItemType,
  AssignWorkOptions,
  MentorApi,
  ReminderCadence,
} from '@wu/api';
import { AuthStore } from '@wu/auth';
import { WuAccessibleDialog, WuIcon, WuShell } from '@wu/ui';

type TargetMode = 'individual' | 'cohort';

// Traces to: L2-MENTOR-008, L2-ENGAGE-008, L2-AUTH-010
@Component({
  selector: 'wu-mentor-assign-work',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WuShell, WuIcon, WuAccessibleDialog],
  templateUrl: './mentor-assign-work.html',
  styles: `
    .assign-layout {
      display: grid;
      grid-template-columns: minmax(0, 1.5fr) minmax(16rem, 0.6fr);
      gap: var(--wu-s6);
      align-items: start;
    }
    .summary-card {
      position: sticky;
      top: var(--wu-s4);
    }
    .recipient-list {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: var(--wu-s2) var(--wu-s4);
    }
    .recipient-list .wu-check {
      margin-bottom: 0;
    }
    .review-button,
    .confirm-button {
      min-height: 46px;
    }
    .selection-summary {
      overflow-wrap: anywhere;
    }
    @media (max-width: 54rem) {
      .assign-layout {
        grid-template-columns: 1fr;
      }
      .summary-card {
        position: static;
      }
    }
    @media (max-width: 34rem) {
      .recipient-list {
        grid-template-columns: 1fr;
      }
      .review-button {
        width: 100%;
      }
      .wu-modal__footer .wu-btn {
        flex: 1 1 100%;
      }
    }
  `,
})
export class MentorAssignWorkPage {
  protected readonly auth = inject(AuthStore);
  private readonly api = inject(MentorApi);

  protected readonly options = signal<AssignWorkOptions | null>(null);
  protected readonly itemType = signal<AssignWorkItemType>('Lesson');
  protected readonly itemId = signal('');
  protected readonly targetMode = signal<TargetMode>('individual');
  protected readonly youthIds = signal<string[]>([]);
  protected readonly cohortId = signal('');
  protected readonly dueDate = signal(this.dateDaysFromNow(7));
  protected readonly reminderCadence = signal<ReminderCadence | ''>('');
  protected readonly message = signal('');
  protected readonly reviewing = signal(false);
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  protected readonly filteredItems = computed(() =>
    (this.options()?.items ?? []).filter((item) => item.itemType === this.itemType()),
  );
  protected readonly selectedItem = computed(() =>
    this.options()?.items.find((item) => item.itemId === this.itemId()),
  );
  protected readonly recipientCount = computed(() => {
    if (this.targetMode() === 'individual') return this.youthIds().length;
    return (
      this.options()?.cohorts.find((cohort) => cohort.cohortId === this.cohortId())?.youthCount ?? 0
    );
  });
  protected readonly recipientSummary = computed(() => {
    if (this.targetMode() === 'cohort') {
      return (
        this.options()?.cohorts.find((cohort) => cohort.cohortId === this.cohortId())?.name ??
        'No cohort selected'
      );
    }
    const selected = new Set(this.youthIds());
    return (
      this.options()
        ?.youth.filter((youth) => selected.has(youth.youthId))
        .map((youth) => youth.displayName)
        .join(', ') || 'No youth selected'
    );
  });

  constructor() {
    this.api.assignWorkOptions().subscribe({
      next: (options) => {
        this.options.set(options);
        this.selectFirstItem();
        if (options.cohorts.length === 1) this.cohortId.set(options.cohorts[0].cohortId);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected setItemType(value: string): void {
    this.itemType.set(value as AssignWorkItemType);
    this.selectFirstItem();
  }

  protected setTargetMode(value: TargetMode): void {
    this.targetMode.set(value);
  }

  protected toggleYouth(youthId: string, checked: boolean): void {
    this.youthIds.update((ids) =>
      checked ? [...new Set([...ids, youthId])] : ids.filter((id) => id !== youthId),
    );
  }

  protected review(): void {
    this.errorMessage.set(null);
    if (!this.itemId()) {
      this.errorMessage.set('Choose an item to assign.');
      return;
    }
    if (this.recipientCount() === 0) {
      this.errorMessage.set('Choose at least one youth or a whole cohort.');
      return;
    }
    if (!this.dueDate()) {
      this.errorMessage.set('Choose a due date.');
      return;
    }
    this.reviewing.set(true);
  }

  protected cancelReview(): void {
    this.reviewing.set(false);
  }

  protected confirm(): void {
    const item = this.selectedItem();
    if (!item || this.saving()) return;
    this.saving.set(true);
    this.errorMessage.set(null);
    this.api
      .assignWork({
        itemType: this.itemType(),
        itemId: item.itemId,
        youthIds: this.targetMode() === 'individual' ? this.youthIds() : null,
        cohortId: this.targetMode() === 'cohort' ? this.cohortId() : null,
        dueDate: this.dueDate() ? `${this.dueDate()}T12:00:00.000Z` : null,
        reminderCadence: this.reminderCadence() || null,
        message: this.optional(this.message()),
      })
      .subscribe({
        next: (result) => {
          this.saving.set(false);
          this.reviewing.set(false);
          this.statusMessage.set(`Assigned ${item.title} to ${result.created} youth.`);
        },
        error: (error: unknown) => this.fail(error),
      });
  }

  protected itemLabel(item: AssignableWorkItem): string {
    return `${item.title} — ${item.moduleName}`;
  }

  private selectFirstItem(): void {
    this.itemId.set(this.filteredItems()[0]?.itemId ?? '');
  }

  private optional(value: string): string | null {
    return value.trim() || null;
  }

  private dateDaysFromNow(days: number): string {
    const date = new Date();
    date.setUTCDate(date.getUTCDate() + days);
    return date.toISOString().slice(0, 10);
  }

  private fail(error: unknown): void {
    this.saving.set(false);
    this.reviewing.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }
}
