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

export interface MentorMessageConversation {
  youthId: string;
  youthName: string;
  cohortId: string | null;
  cohortName: string | null;
  unreadCount: number;
  lastMessagePreview: string | null;
  lastMessageAt: string | null;
}

export interface MentorBroadcastGroup {
  cohortId: string;
  cohortName: string;
  recipientCount: number;
}

export interface MentorMessageInbox {
  conversations: MentorMessageConversation[];
  groups: MentorBroadcastGroup[];
}

export interface BroadcastResult {
  sent: number;
}
