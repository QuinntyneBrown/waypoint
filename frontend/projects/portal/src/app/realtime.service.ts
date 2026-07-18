import { effect, inject, Injectable, signal } from '@angular/core';
import { API_BASE_URL, MessageView, NotificationView } from '@wu/api';
import { SessionStore } from '@wu/auth';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';

export type RealtimeState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

// Traces to: L2-NFR-005, L2-ENGAGE-007, L2-ENGAGE-008
@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly session = inject(SessionStore);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly messageEvents = new Subject<MessageView>();
  private readonly notificationEvents = new Subject<NotificationView>();
  private readonly reconnectedEvents = new Subject<void>();
  private connection: HubConnection | null = null;
  private connectionFor: string | null = null;
  private retryTimer: ReturnType<typeof setTimeout> | null = null;

  readonly state = signal<RealtimeState>('disconnected');
  readonly messages$ = this.messageEvents.asObservable();
  readonly notifications$ = this.notificationEvents.asObservable();
  readonly reconnected$ = this.reconnectedEvents.asObservable();

  constructor() {
    effect(() => {
      const user = this.session.user();
      const token = this.session.accessToken();
      if (user && token) void this.connect(user.userId);
      else void this.disconnect();
    });
  }

  private async connect(userId: string): Promise<void> {
    if (this.connectionFor !== userId) {
      await this.disconnect();
      this.connectionFor = userId;
      this.connection = new HubConnectionBuilder()
        .withUrl(`${this.apiBase}/hubs/realtime`, {
          accessTokenFactory: () => this.session.accessToken() ?? '',
        })
        .withAutomaticReconnect([0, 1_000, 5_000, 10_000])
        .configureLogging(LogLevel.Warning)
        .build();
      this.connection.on('message', (message: MessageView) => this.messageEvents.next(message));
      this.connection.on('notification', (notification: NotificationView) =>
        this.notificationEvents.next({ ...notification, read: false }),
      );
      this.connection.onreconnecting(() => this.state.set('reconnecting'));
      this.connection.onreconnected(() => {
        this.state.set('connected');
        this.reconnectedEvents.next();
      });
      this.connection.onclose(() => {
        this.state.set('disconnected');
        this.scheduleRetry();
      });
    }

    await this.start();
  }

  private async start(): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Disconnected) return;
    this.clearRetry();
    this.state.set('connecting');
    try {
      await this.connection.start();
      this.state.set('connected');
    } catch {
      this.state.set('disconnected');
      this.scheduleRetry();
    }
  }

  private scheduleRetry(): void {
    if (this.retryTimer || !this.session.user()) return;
    this.retryTimer = setTimeout(() => {
      this.retryTimer = null;
      void this.start();
    }, 5_000);
  }

  private async disconnect(): Promise<void> {
    this.clearRetry();
    const connection = this.connection;
    this.connection = null;
    this.connectionFor = null;
    this.state.set('disconnected');
    if (connection && connection.state !== HubConnectionState.Disconnected) {
      try {
        await connection.stop();
      } catch {
        // Session teardown remains best effort during navigation/sign-out.
      }
    }
  }

  private clearRetry(): void {
    if (this.retryTimer) {
      clearTimeout(this.retryTimer);
      this.retryTimer = null;
    }
  }
}
