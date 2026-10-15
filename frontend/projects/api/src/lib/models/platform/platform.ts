export interface UpdateProfileRequest {
  displayName: string;
  email: string;
  pronouns?: string | null;
}

export interface MyProfile {
  displayName: string;
  email: string;
  pronouns: string | null;
  photoUrl: string | null;
  hasPhoto: boolean;
}

export interface ProfilePhoto {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
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
  screenshotFileId: string | null;
}

export interface SupportResult {
  id: string;
}

export interface SupportScreenshot {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  url: string;
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

export interface ComposeAnnouncementRequest {
  title: string;
  message: string;
  audienceScope: 'org' | 'cohort';
  cohortId: string | null;
  roleTargeting: AnnouncementRoleTarget;
  scheduledAt: string | null;
}

export type AnnouncementRoleTarget =
  'Youth' | 'YouthGuardians' | 'Guardians' | 'Mentors' | 'Everyone';

export interface AnnouncementResult {
  id: string;
  sentAt: string | null;
}

export interface ManagedAnnouncement {
  id: string;
  title: string;
  message: string;
  audienceScope: 'org' | 'cohort';
  cohortId: string | null;
  cohortName: string | null;
  roleTargeting: AnnouncementRoleTarget;
  scheduledAt: string | null;
  sentAt: string | null;
  createdAt: string;
  recipients: number;
  reads: number;
  readRate: number;
}
