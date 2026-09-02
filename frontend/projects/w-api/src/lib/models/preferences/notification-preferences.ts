export interface NotificationPreference {
  eventKey: string;
  inApp: boolean;
  email: boolean;
}

/** A notification category shown in the UI, keyed by the backend event key. */
export interface NotificationCategory {
  eventKey: string;
  label: string;
  description: string;
}

/**
 * Canonical notification categories and their onboarding defaults (L2-AUTH-011
 * AC3 / L2-PLATFORM-003): actionable events on, announcements off.
 * Event keys are free strings on the backend; the frontend
 * owns this vocabulary.
 */
export const NOTIFICATION_CATEGORIES: readonly NotificationCategory[] = [
  {
    eventKey: 'lesson-reminders',
    label: 'Lesson reminders',
    description: 'Nudges to keep your journey moving.',
  },
  {
    eventKey: 'mentor-messages',
    label: 'Mentor messages',
    description: 'When your mentor sends you a message.',
  },
  {
    eventKey: 'session-reminders',
    label: 'Session reminders',
    description: 'A reminder before sessions you plan to attend.',
  },
  {
    eventKey: 'work-graded',
    label: 'Work graded',
    description: 'When a mentor reviews your submitted work.',
  },
  {
    eventKey: 'badge-earned',
    label: 'Badges earned',
    description: 'When you earn a new achievement badge.',
  },
  {
    eventKey: 'at-risk-alert',
    label: 'At-risk alerts',
    description: 'When an assigned youth needs follow-up.',
  },
  {
    eventKey: 'approval-needed',
    label: 'Approval needed',
    description: 'When submitted work is ready for your review.',
  },
  {
    eventKey: 'announcements',
    label: 'Announcements',
    description: 'Program-wide news and updates.',
  },
];

/** Default in-app on/off per category at onboarding. */
export const NOTIFICATION_DEFAULTS: Readonly<Record<string, boolean>> = {
  'lesson-reminders': true,
  'mentor-messages': true,
  'session-reminders': true,
  'work-graded': true,
  'badge-earned': true,
  'at-risk-alert': true,
  'approval-needed': true,
  announcements: false,
};
