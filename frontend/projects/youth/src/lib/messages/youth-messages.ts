import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiError, MessagingApi, YouthMessageThread } from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WShell } from '@waypoint/components';
import { RealtimeService } from '@waypoint/core/realtime';

// Traces to: L2-ENGAGE-007, L2-NFR-005
@Component({
  selector: 'w-youth-messages',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, DatePipe, ReactiveFormsModule],
  templateUrl: './youth-messages.html',
  styles: `
    .thread-card {
      overflow: hidden;
    }
    .thread-log {
      display: flex;
      flex-direction: column;
      gap: var(--w-sp-3);
      min-height: 12rem;
      max-height: 34rem;
      overflow-y: auto;
      margin: 0;
      padding: var(--w-sp-5);
      list-style: none;
      background: var(--w-g100);
    }
    .message-row {
      display: flex;
    }
    .message-row--mine {
      justify-content: flex-end;
    }
    .message-bubble {
      width: fit-content;
      max-width: 75%;
      padding: var(--w-sp-3) var(--w-sp-4);
      border: 1px solid var(--w-border);
      border-radius: var(--w-r-card);
      background: var(--w-white);
      overflow-wrap: anywhere;
    }
    .message-row--mine .message-bubble {
      background: var(--w-lime-soft);
    }
    .message-body {
      white-space: pre-wrap;
    }
    .composer {
      padding: var(--w-sp-5);
      border-top: 1px solid var(--w-border);
    }
    @media (max-width: 40rem) {
      .message-bubble {
        max-width: 90%;
      }
      .thread-log,
      .composer {
        padding: var(--w-sp-3);
      }
    }
  `,
})
export class YouthMessagesPage {
  protected readonly auth = inject(AuthStore);
  protected readonly realtime = inject(RealtimeService);
  private readonly api = inject(MessagingApi);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly thread = signal<YouthMessageThread | undefined>(undefined);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly liveStatus = signal<string | null>(null);
  protected readonly sending = signal(false);
  protected readonly form = this.fb.nonNullable.group({
    body: ['', [Validators.required, Validators.maxLength(4000)]],
  });

  constructor() {
    this.load();
    this.realtime.messages$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((message) => {
      const view = this.thread();
      const userId = this.auth.user()?.userId;
      if (!view?.mentorId || !userId) return;
      if (message.senderId !== view.mentorId || message.recipientId !== userId) return;
      this.liveStatus.set('New message from your mentor.');
      this.load(false);
    });
    this.realtime.reconnected$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.liveStatus.set('Live updates reconnected.');
      this.load(false);
    });
  }

  protected send(): void {
    const mentorId = this.thread()?.mentorId;
    if (!mentorId || this.form.invalid || this.sending()) {
      this.form.markAllAsTouched();
      return;
    }
    const body = this.form.controls.body.value.trim();
    if (!body) return;

    this.sending.set(true);
    this.errorMessage.set(null);
    this.api.send(mentorId, body).subscribe({
      next: () => {
        this.form.reset();
        this.sending.set(false);
        this.liveStatus.set('Message sent.');
        this.load(false);
      },
      error: (error: unknown) => {
        this.sending.set(false);
        this.errorMessage.set(ApiError.from(error).title);
      },
    });
  }

  private load(showLoading = true): void {
    if (showLoading) this.thread.set(undefined);
    this.api.mine().subscribe({
      next: (thread) => this.thread.set(thread),
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }
}
