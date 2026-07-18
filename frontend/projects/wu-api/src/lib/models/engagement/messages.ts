// Traces to: L2-ENGAGE-007, L2-NFR-005
export interface MessageView {
  id: string;
  senderId: string;
  recipientId: string;
  body: string;
  sentAt: string;
  readAt: string | null;
}

export interface YouthMessageThread {
  mentorId: string | null;
  mentorName: string | null;
  unreadCount: number;
  messages: MessageView[];
}

export interface MessageResult {
  id: string;
}
