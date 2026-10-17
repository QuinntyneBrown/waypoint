import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  ApiError,
  MentorBroadcastGroup,
  MentorMessageConversation,
  MentorMessageInbox,
  MessageView,
  MessagingService,
} from '@waypoint/api';
import { AuthStore } from '@waypoint/auth';
import { WAccessibleDialog, WIcon, WShell } from '@waypoint/components';
import { RealtimeService } from '@waypoint/core/realtime';

type MessageMode = 'thread' | 'broadcast';

// Traces to: L2-ENGAGE-007, L2-ENGAGE-008, L2-NFR-005, L2-AUTH-010
@Component({
  selector: 'w-mentor-messages',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WShell, WIcon, WAccessibleDialog, DatePipe, ReactiveFormsModule],
  templateUrl: './mentor-messages.html',
  styles: `
    .messages-layout {
      display: grid;
      grid-template-columns: minmax(17rem, 0.7fr) minmax(0, 1.5fr);
      min-height: 38rem;
      overflow: hidden;
    }
    .inbox-panel {
      border-right: 1px solid var(--w-border);
      background: var(--w-white);
    }
    .inbox-head,
    .conversation-head,
    .composer,
    .broadcast-panel {
      padding: var(--w-s4);
    }
    .conversation-list {
      margin: 0;
      padding: 0;
      list-style: none;
      border-top: 1px solid var(--w-border);
    }
    .conversation-button {
      display: block;
      width: 100%;
      min-height: 62px;
      padding: var(--w-s3) var(--w-s4);
      border: 0;
      border-bottom: 1px solid var(--w-border);
      background: transparent;
      color: inherit;
      text-align: left;
      cursor: pointer;
    }
    .conversation-button:hover,
    .conversation-button.is-active {
      background: var(--w-lime-soft);
    }
    .conversation-button:focus-visible {
      outline: 3px solid var(--w-ink);
      outline-offset: -3px;
    }
    .conversation-preview {
      display: block;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .thread-panel {
      display: flex;
      min-width: 0;
      flex-direction: column;
      background: var(--w-g100);
    }
    .conversation-head {
      border-bottom: 1px solid var(--w-border);
      background: var(--w-white);
    }
    .thread-log {
      flex: 1 1 auto;
      min-height: 18rem;
      max-height: 32rem;
      padding: var(--w-s5);
      overflow-y: auto;
    }
    .message-body {
      white-space: pre-wrap;
    }
    .composer {
      border-top: 1px solid var(--w-border);
      background: var(--w-white);
    }
    .mobile-back {
      display: none;
    }
    @media (max-width: 46rem) {
      .messages-layout {
        display: block;
        min-height: 0;
      }
      .inbox-panel {
        border-right: 0;
      }
      .messages-layout.show-thread .inbox-panel {
        display: none;
      }
      .messages-layout:not(.show-thread) .thread-panel {
        display: none;
      }
      .mobile-back {
        display: inline-flex;
      }
      .thread-log {
        max-height: none;
      }
    }
  `,
})
export class MentorMessagesPage {
  protected readonly auth = inject(AuthStore);
  protected readonly realtime = inject(RealtimeService);
  private readonly api = inject(MessagingService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly inbox = signal<MentorMessageInbox | undefined>(undefined);
  protected readonly selectedYouthId = signal<string | null>(null);
  protected readonly mode = signal<MessageMode>('thread');
  protected readonly thread = signal<MessageView[] | undefined>(undefined);
  protected readonly loadingThread = signal(false);
  protected readonly sending = signal(false);
  protected readonly broadcasting = signal(false);
  protected readonly reviewingBroadcast = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly liveStatus = signal<string | null>(null);
  protected readonly search = this.fb.nonNullable.control('');
  private readonly searchQuery = signal('');
  protected readonly composer = this.fb.nonNullable.group({
    body: ['', [Validators.required, Validators.maxLength(4000)]],
  });
  protected readonly broadcastForm = this.fb.nonNullable.group({
    cohortId: ['', Validators.required],
    body: ['', [Validators.required, Validators.maxLength(4000)]],
  });

  protected readonly selectedConversation = computed(() =>
    this.inbox()?.conversations.find((item) => item.youthId === this.selectedYouthId()),
  );
  protected readonly filteredConversations = computed(() => {
    const query = this.searchQuery().trim().toLocaleLowerCase();
    const conversations = this.inbox()?.conversations ?? [];
    return query
      ? conversations.filter(
          (item) =>
            item.youthName.toLocaleLowerCase().includes(query) ||
            item.cohortName?.toLocaleLowerCase().includes(query),
        )
      : conversations;
  });
  protected readonly totalUnread = computed(() =>
    (this.inbox()?.conversations ?? []).reduce((total, item) => total + item.unreadCount, 0),
  );

  constructor() {
    this.search.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => this.searchQuery.set(value));
    this.loadInbox(true);
    this.realtime.messages$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((message) => {
      const mentorId = this.auth.user()?.userId;
      const sender = this.inbox()?.conversations.find((item) => item.youthId === message.senderId);
      if (!mentorId || message.recipientId !== mentorId || !sender) return;
      this.liveStatus.set(`New message from ${sender.youthName}.`);
      if (this.selectedYouthId() === sender.youthId && this.mode() === 'thread') {
        this.thread.update((current) => this.mergeMessages(current ?? [], [message]));
        this.loadThread(sender.youthId, false);
      } else {
        this.loadInbox(false, false);
      }
    });
    this.realtime.reconnected$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.liveStatus.set('Live updates reconnected.');
      if (this.selectedYouthId()) this.loadThread(this.selectedYouthId()!, false);
      else this.loadInbox(false, false);
    });
  }

  protected selectConversation(conversation: MentorMessageConversation): void {
    this.mode.set('thread');
    this.selectedYouthId.set(conversation.youthId);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { youthId: conversation.youthId },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
    this.loadThread(conversation.youthId);
  }

  protected openBroadcast(): void {
    this.mode.set('broadcast');
    this.selectedYouthId.set(null);
    this.thread.set(undefined);
    if (!this.broadcastForm.controls.cohortId.value) {
      this.broadcastForm.controls.cohortId.setValue(this.inbox()?.groups[0]?.cohortId ?? '');
    }
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { youthId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  protected showInbox(): void {
    this.mode.set('thread');
    this.selectedYouthId.set(null);
    this.thread.set(undefined);
  }

  protected selectedGroup(): MentorBroadcastGroup | undefined {
    return this.inbox()?.groups.find(
      (item) => item.cohortId === this.broadcastForm.controls.cohortId.value,
    );
  }

  protected send(): void {
    const youthId = this.selectedYouthId();
    const body = this.composer.controls.body.value.trim();
    if (!youthId || !body || this.composer.invalid || this.sending()) {
      this.composer.markAllAsTouched();
      return;
    }
    this.sending.set(true);
    this.errorMessage.set(null);
    this.api.send(youthId, body).subscribe({
      next: () => {
        this.composer.reset();
        this.sending.set(false);
        this.liveStatus.set('Message sent.');
        this.loadThread(youthId, false);
      },
      error: (error: unknown) => this.handleError(error, this.sending),
    });
  }

  protected reviewBroadcast(): void {
    const body = this.broadcastForm.controls.body.value.trim();
    if (!body || this.broadcastForm.invalid) {
      this.broadcastForm.markAllAsTouched();
      return;
    }
    this.reviewingBroadcast.set(true);
  }

  protected confirmBroadcast(): void {
    const { cohortId } = this.broadcastForm.getRawValue();
    const body = this.broadcastForm.controls.body.value.trim();
    if (!cohortId || !body || this.broadcasting()) return;
    this.broadcasting.set(true);
    this.errorMessage.set(null);
    this.api.broadcast(cohortId, body).subscribe({
      next: (result) => {
        this.broadcasting.set(false);
        this.reviewingBroadcast.set(false);
        this.broadcastForm.controls.body.reset();
        this.liveStatus.set(
          `Broadcast sent as ${result.sent} personal ${result.sent === 1 ? 'message' : 'messages'}.`,
        );
        this.loadInbox(false, false);
      },
      error: (error: unknown) => {
        this.reviewingBroadcast.set(false);
        this.handleError(error, this.broadcasting);
      },
    });
  }

  private loadInbox(showLoading: boolean, chooseConversation = true): void {
    if (showLoading) this.inbox.set(undefined);
    this.api.mentorInbox().subscribe({
      next: (inbox) => {
        this.inbox.set(inbox);
        if (!chooseConversation || this.mode() === 'broadcast') return;
        const requested = this.route.snapshot.queryParamMap.get('youthId');
        const youthId =
          (requested && inbox.conversations.some((item) => item.youthId === requested)
            ? requested
            : null) ??
          (this.selectedYouthId() &&
          inbox.conversations.some((item) => item.youthId === this.selectedYouthId())
            ? this.selectedYouthId()
            : null) ??
          inbox.conversations[0]?.youthId ??
          null;
        if (youthId) {
          this.selectedYouthId.set(youthId);
          this.loadThread(youthId);
        }
      },
      error: (error: unknown) => this.errorMessage.set(ApiError.from(error).title),
    });
  }

  private loadThread(youthId: string, showLoading = true): void {
    if (showLoading) {
      this.loadingThread.set(true);
      this.thread.set(undefined);
    }
    this.api.thread(youthId).subscribe({
      next: (messages) => {
        if (this.selectedYouthId() === youthId && this.mode() === 'thread') {
          this.thread.update((current) => this.mergeMessages(current ?? [], messages));
          this.loadingThread.set(false);
        }
        this.inbox.update((current) =>
          current
            ? {
                ...current,
                conversations: current.conversations.map((item) =>
                  item.youthId === youthId ? { ...item, unreadCount: 0 } : item,
                ),
              }
            : current,
        );
        this.loadInbox(false, false);
      },
      error: (error: unknown) => {
        if (this.selectedYouthId() === youthId) {
          this.loadingThread.set(false);
          this.errorMessage.set(ApiError.from(error).title);
        }
      },
    });
  }

  private mergeMessages(current: MessageView[], incoming: MessageView[]): MessageView[] {
    const byId = new Map(current.map((message) => [message.id, message]));
    for (const message of incoming) byId.set(message.id, message);
    return [...byId.values()].sort(
      (left, right) => Date.parse(left.sentAt) - Date.parse(right.sentAt),
    );
  }

  private handleError(error: unknown, state: { set(value: boolean): void }): void {
    state.set(false);
    this.errorMessage.set(ApiError.from(error).title);
  }
}
