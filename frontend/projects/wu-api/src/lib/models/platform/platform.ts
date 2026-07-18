export interface UpdateProfileRequest {
  displayName: string;
  email: string;
  pronouns?: string | null;
  photoUrl?: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface SupportRequest {
  category: string;
  description: string;
  replyToEmail: string;
  screenshotUrl?: string | null;
}

export interface SupportResult {
  id: string;
}

export const SUPPORT_CATEGORIES = [
  'Account & sign-in',
  'A technical problem',
  'A question about the program',
  'Something else',
] as const;

export interface NotificationView {
  id: string;
  eventKey: string;
  title: string;
  body: string;
  linkPath: string | null;
  read: boolean;
  createdAt: string;
}

export interface AnnouncementView {
  id: string;
  title: string;
  message: string;
  sentAt: string | null;
  read: boolean;
}
