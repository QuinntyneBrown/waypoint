import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ApiError,
  AtRiskYouth,
  FlagResolutionReason,
  MentorService,
  MentorRosterYouth,
  MessagingService,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import {
  WButton,
  WFormField,
  WIcon,
  WLabel,
  WSelect,
  WShell,
  WTextarea,
  WTextField,
  WAlert,
} from '@waypoint/components';

// Traces to: L2-MENTOR-007, L2-ENGAGE-007, L2-AUTH-010
@Component({
  selector: 'w-mentor-flags',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    WShell,
    WIcon,
    RouterLink,
    DatePipe,
    WButton,
    WFormField,
    WLabel,
    WTextField,
    WTextarea,
    WSelect,
    WAlert,
  ],
  templateUrl: './mentor-flags.html',
  styles: `
    .raise-grid {
      display: grid;
      grid-template-columns: minmax(12rem, 0.75fr) minmax(16rem, 1.5fr) auto;
      gap: var(--w-s4);
      align-items: end;
    }
    .flag-list {
      display: grid;
      gap: var(--w-s4);
    }
    .flag-card {
      overflow: hidden;
    }
    .reason-list {
      margin: 0;
      padding-left: 1.25rem;
    }
    .reason-list li {
      overflow-wrap: anywhere;
    }
    .inline-action {
      padding: var(--w-s4);
      border-top: 1px solid var(--w-border);
      background: var(--w-g100);
    }
    .action-field {
      max-width: 38rem;
    }
    .flag-actions .w-btn,
    .raise-button {
      min-height: 44px;
    }
    @media (max-width: 48rem) {
      .raise-grid {
        grid-template-columns: 1fr;
      }
      .raise-button {
        width: 100%;
      }
    }
  `,
})
export class MentorFlagsPage {
  protected readonly auth = inject(AuthStore);
  private readonly mentorApi = inject(MentorService);
  private readonly messagingApi = inject(MessagingService);

  protected readonly youth = signal<MentorRosterYouth[]>([]);
  protected readonly flags = signal<AtRiskYouth[] | null>(null);
  protected readonly selectedYouthId = signal('');
  protected readonly manualReason = signal('');
  protected readonly messageFlagId = signal<string | null>(null);
  protected readonly messageBody = signal('');
  protected readonly resolvingFlagId = signal<string | null>(null);
  protected readonly resolutionReason = signal<FlagResolutionReason>('Reconnected');
  protected readonly busy = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly statusMessage = signal<string | null>(null);

  constructor() {
    this.mentorApi.roster('all').subscribe({
      next: (youth) => {
        this.youth.set(youth);
        if (youth.length === 1) this.selectedYouthId.set(youth[0].id);
      },
      error: (error: unknown) => this.fail(error),
    });
    this.loadFlags();
  }

  protected raise(): void {
    const youthId = this.selectedYouthId();
    const reason = this.manualReason().trim();
    if (!youthId || !reason) {
      this.errorMessage.set('Choose a youth and enter a reason before raising a flag.');
      return;
    }
    this.startAction();
    this.mentorApi.raiseFlag(youthId, reason).subscribe({
      next: () => {
        this.finishAction(`Flag raised for ${this.youthName(youthId)}.`);
        this.manualReason.set('');
        this.loadFlags();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected toggleMessage(flag: AtRiskYouth): void {
    this.resolvingFlagId.set(null);
    this.messageBody.set('');
    this.messageFlagId.set(this.messageFlagId() === flag.flagId ? null : flag.flagId);
  }

  protected sendMessage(flag: AtRiskYouth): void {
    const body = this.messageBody().trim();
    if (!body) {
      this.errorMessage.set('Enter a message before sending.');
      return;
    }
    this.startAction();
    this.messagingApi.send(flag.youthId, body).subscribe({
      next: () => {
        this.finishAction(`Message sent to ${flag.displayName}.`);
        this.messageBody.set('');
        this.messageFlagId.set(null);
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected startResolve(flag: AtRiskYouth): void {
    this.messageFlagId.set(null);
    this.resolutionReason.set('Reconnected');
    this.resolvingFlagId.set(flag.flagId);
  }

  protected cancelResolve(): void {
    this.resolvingFlagId.set(null);
  }

  protected confirmResolve(flag: AtRiskYouth): void {
    this.startAction();
    this.mentorApi.resolveFlag(flag.flagId, this.resolutionReason()).subscribe({
      next: () => {
        this.finishAction(`Flag resolved for ${flag.displayName}.`);
        this.resolvingFlagId.set(null);
        this.loadFlags();
      },
      error: (error: unknown) => this.fail(error),
    });
  }

  protected setResolution(value: string): void {
    this.resolutionReason.set(value as FlagResolutionReason);
  }

  private loadFlags(): void {
    this.mentorApi.flags().subscribe({
      next: (flags) => this.flags.set(flags),
      error: (error: unknown) => this.fail(error),
    });
  }

  private youthName(id: string): string {
    return this.youth().find((youth) => youth.id === id)?.displayName ?? 'the youth';
  }

  private startAction(): void {
    this.busy.set(true);
    this.errorMessage.set(null);
    this.statusMessage.set(null);
  }

  private finishAction(message: string): void {
    this.busy.set(false);
    this.statusMessage.set(message);
  }

  private fail(error: unknown): void {
    this.busy.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }
}
