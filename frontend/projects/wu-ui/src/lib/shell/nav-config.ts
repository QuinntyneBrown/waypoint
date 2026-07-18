export type WuRole = 'youth' | 'mentor' | 'admin' | 'guardian' | 'volunteer';

export interface WuNavItem {
  readonly key: string;
  readonly label: string;
  readonly icon: string;
  /** Angular route path (absolute). */
  readonly path: string;
  readonly badge?: string;
}

/** A section heading in the sidenav. */
export interface WuNavGroup {
  readonly group: string;
}

export type WuNavEntry = WuNavItem | WuNavGroup;

export interface WuRoleNav {
  readonly brandPath: string;
  /** Keys promoted to the mobile bottom nav (in order); empty = no bottom nav. */
  readonly botnav: readonly string[];
  readonly items: readonly WuNavEntry[];
}

export function isNavGroup(entry: WuNavEntry): entry is WuNavGroup {
  return (entry as WuNavGroup).group !== undefined;
}

/**
 * Per-role navigation, mirroring the mock's NAV config (`wordup.js`) but with
 * Angular route paths. Areas beyond Tier A (learning, mentor, admin, …) are
 * present so the shell renders faithfully; their routes land as they are built.
 */
export const WU_NAV: Record<WuRole, WuRoleNav> = {
  youth: {
    brandPath: '/youth/home',
    botnav: ['home', 'journey', 'curriculum', 'messages', 'profile'],
    items: [
      { key: 'home', label: 'Home', icon: 'home', path: '/youth/home' },
      { key: 'journey', label: 'My Journey', icon: 'target', path: '/youth/journey' },
      { key: 'curriculum', label: 'Curriculum', icon: 'book-open', path: '/youth/curriculum' },
      { key: 'badges', label: 'Badges', icon: 'award', path: '/youth/badges' },
      { key: 'schedule', label: 'Schedule', icon: 'calendar', path: '/youth/schedule' },
      { key: 'messages', label: 'Messages', icon: 'message-square', path: '/youth/messages' },
      { key: 'announcements', label: 'Announcements', icon: 'bell', path: '/youth/announcements' },
      { key: 'profile', label: 'Profile', icon: 'user', path: '/settings/profile' },
    ],
  },
  mentor: {
    brandPath: '/mentor/dashboard',
    botnav: [],
    items: [
      { key: 'dashboard', label: 'Dashboard', icon: 'grid', path: '/mentor/dashboard' },
      { key: 'my-youth', label: 'My Youth', icon: 'users', path: '/mentor/my-youth' },
      { key: 'check-ins', label: 'Check-ins', icon: 'clipboard-check', path: '/mentor/check-ins' },
      { key: 'reviews', label: 'Reviews', icon: 'file-text', path: '/mentor/reviews' },
      { key: 'attendance', label: 'Attendance', icon: 'user-check', path: '/mentor/attendance' },
      { key: 'flags', label: 'At-risk', icon: 'flag', path: '/mentor/flags' },
      { group: 'Program' },
      { key: 'curriculum', label: 'Curriculum', icon: 'book-open', path: '/mentor/curriculum' },
      { key: 'calendar', label: 'Calendar', icon: 'calendar', path: '/mentor/calendar' },
      { key: 'messages', label: 'Messages', icon: 'message-square', path: '/mentor/messages' },
    ],
  },
  admin: {
    brandPath: '/admin/dashboard',
    botnav: [],
    items: [
      { key: 'dashboard', label: 'Dashboard', icon: 'grid', path: '/admin/dashboard' },
      { group: 'People' },
      { key: 'people', label: 'People', icon: 'users', path: '/admin/people' },
      { key: 'cohorts', label: 'Cohorts', icon: 'layers', path: '/admin/cohorts' },
      {
        key: 'applications',
        label: 'Applications',
        icon: 'user-plus',
        path: '/admin/applications',
      },
      { key: 'invitations', label: 'Invitations', icon: 'mail', path: '/admin/invitations' },
      { key: 'users-roles', label: 'Users & Roles', icon: 'shield', path: '/admin/users-roles' },
      { group: 'Progress' },
      {
        key: 'cohort-progress',
        label: 'Progress Board',
        icon: 'table',
        path: '/admin/cohort-progress',
      },
      { key: 'reports', label: 'Reports', icon: 'bar-chart', path: '/admin/reports' },
      { key: 'attendance', label: 'Attendance', icon: 'user-check', path: '/admin/attendance' },
      { group: 'Curriculum' },
      { key: 'tracks', label: 'Curriculum', icon: 'book-open', path: '/admin/tracks' },
      { key: 'badges', label: 'Badges', icon: 'award', path: '/admin/badges' },
      { key: 'progression', label: 'Progression', icon: 'target', path: '/admin/progression' },
      { group: 'Program' },
      { key: 'calendar', label: 'Calendar', icon: 'calendar', path: '/admin/calendar' },
      { key: 'announcements', label: 'Announcements', icon: 'bell', path: '/admin/announcements' },
      { group: 'System' },
      { key: 'settings', label: 'Settings', icon: 'settings', path: '/admin/settings' },
      { key: 'audit', label: 'Audit Log', icon: 'activity', path: '/admin/audit' },
    ],
  },
  guardian: {
    brandPath: '/guardian/home',
    botnav: ['home', 'progress', 'announcements', 'consent'],
    items: [
      { key: 'home', label: 'Home', icon: 'home', path: '/guardian/home' },
      { key: 'progress', label: 'My Youth', icon: 'trending-up', path: '/guardian/progress' },
      { key: 'announcements', label: 'News', icon: 'bell', path: '/guardian/announcements' },
      { key: 'consent', label: 'Consent', icon: 'shield', path: '/guardian/consent' },
    ],
  },
  volunteer: {
    brandPath: '/volunteer/home',
    botnav: ['home', 'attendance', 'schedule'],
    items: [
      { key: 'home', label: 'Today', icon: 'home', path: '/volunteer/home' },
      { key: 'attendance', label: 'Attendance', icon: 'user-check', path: '/volunteer/attendance' },
      { key: 'schedule', label: 'Schedule', icon: 'calendar', path: '/volunteer/schedule' },
    ],
  },
};

export const WU_ROLE_LABEL: Record<WuRole, string> = {
  youth: 'Youth Leader',
  mentor: 'Mentor',
  admin: 'Director',
  guardian: 'Guardian',
  volunteer: 'Volunteer',
};
