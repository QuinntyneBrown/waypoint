import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@w/auth';
import { consentGuard } from './guards/consent.guard';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/sign-in' },
  {
    path: 'invite',
    title: 'Accept invitation · Waypoint',
    loadComponent: () =>
      import('./auth/invite-accept/invite-accept').then((module) => module.InviteAcceptPage),
  },
  {
    path: 'consent/respond',
    title: 'Guardian consent · Waypoint',
    loadComponent: () =>
      import('./states/requested-guardian-consent/requested-guardian-consent').then(
        (module) => module.RequestedGuardianConsentPage,
      ),
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'invite',
        title: 'Accept invitation · Waypoint',
        loadComponent: () =>
          import('./auth/invite-accept/invite-accept').then((module) => module.InviteAcceptPage),
      },
      {
        path: 'sign-in',
        title: 'Sign in · Waypoint',
        loadComponent: () => import('./auth/sign-in/sign-in').then((m) => m.SignIn),
      },
      {
        path: 'forgot-password',
        title: 'Reset password · Waypoint',
        loadComponent: () =>
          import('./auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
      },
      {
        path: 'reset-link-sent',
        title: 'Check your email · Waypoint',
        loadComponent: () =>
          import('./auth/reset-link-sent/reset-link-sent').then((m) => m.ResetLinkSent),
      },
      {
        path: 'reset-password',
        title: 'Set a new password · Waypoint',
        loadComponent: () =>
          import('./auth/reset-password/reset-password').then((m) => m.ResetPassword),
      },
      {
        path: 'reset-link-invalid',
        title: 'Reset link · Waypoint',
        loadComponent: () =>
          import('./auth/reset-link-invalid/reset-link-invalid').then((m) => m.ResetLinkInvalid),
      },
      {
        path: 'reset-success',
        title: 'Password updated · Waypoint',
        loadComponent: () =>
          import('./auth/reset-success/reset-success').then((m) => m.ResetSuccess),
      },
      {
        path: 'account-locked',
        title: 'Account locked · Waypoint',
        loadComponent: () =>
          import('./states/account-locked/account-locked').then((m) => m.AccountLocked),
      },
      {
        path: 'account-deactivated',
        title: 'Account inactive · Waypoint',
        loadComponent: () =>
          import('./states/account-deactivated/account-deactivated').then(
            (m) => m.AccountDeactivated,
          ),
      },
    ],
  },
  {
    path: 'onboarding',
    title: 'Set up your profile · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('./onboarding/onboarding').then((m) => m.Onboarding),
  },
  {
    path: 'youth',
    canActivate: [authGuard, consentGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Home · Waypoint',
        loadComponent: () => import('./features/youth/home/youth-home').then((m) => m.YouthHome),
      },
      {
        path: 'curriculum',
        title: 'Curriculum · Waypoint',
        loadComponent: () =>
          import('./features/youth/curriculum/youth-curriculum').then(
            (module) => module.YouthCurriculumPage,
          ),
      },
      {
        path: 'journey',
        title: 'My journey · Waypoint',
        loadComponent: () =>
          import('./features/youth/journey/youth-journey').then(
            (module) => module.YouthJourneyPage,
          ),
      },
      {
        path: 'badges',
        title: 'Badges · Waypoint',
        loadComponent: () =>
          import('./features/youth/badges/youth-badges').then((module) => module.YouthBadgesPage),
      },
      {
        path: 'progress',
        title: 'My progress · Waypoint',
        loadComponent: () =>
          import('./features/youth/progress/youth-progress').then(
            (module) => module.YouthProgressPage,
          ),
      },
      {
        path: 'schedule',
        title: 'My schedule · Waypoint',
        loadComponent: () =>
          import('./features/youth/schedule/youth-schedule').then(
            (module) => module.YouthSchedulePage,
          ),
      },
      {
        path: 'messages',
        title: 'Messages · Waypoint',
        loadComponent: () =>
          import('./features/youth/messages/youth-messages').then(
            (module) => module.YouthMessagesPage,
          ),
      },
      {
        path: 'coding/:id',
        title: 'Coding lesson · Waypoint',
        loadComponent: () =>
          import('./features/youth/coding/youth-coding').then((module) => module.YouthCodingPage),
      },
      {
        path: 'lessons/:id/reflection',
        title: 'Reflection · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/youth/reflection/youth-reflection').then(
            (module) => module.YouthReflectionPage,
          ),
      },
      {
        path: 'lessons/:id',
        title: 'Lesson · Waypoint',
        loadComponent: () =>
          import('./features/youth/lesson/youth-lesson').then((module) => module.YouthLessonPage),
      },
      {
        path: 'quizzes/:id/attempt',
        title: 'Quiz attempt · Waypoint',
        loadComponent: () =>
          import('./features/youth/quiz-attempt/youth-quiz-attempt').then(
            (module) => module.YouthQuizAttemptPage,
          ),
      },
      {
        path: 'quizzes/:id',
        title: 'Quiz · Waypoint',
        loadComponent: () =>
          import('./features/youth/quiz-intro/youth-quiz-intro').then(
            (module) => module.YouthQuizIntroPage,
          ),
      },
      {
        path: 'quiz-attempts/:id/result',
        title: 'Quiz result · Waypoint',
        loadComponent: () =>
          import('./features/youth/quiz-result/youth-quiz-result').then(
            (module) => module.YouthQuizResultPage,
          ),
      },
      {
        path: 'assignments/:id',
        title: 'Assignment · Waypoint',
        loadComponent: () =>
          import('./features/youth/assignment/youth-assignment').then(
            (module) => module.YouthAssignmentPage,
          ),
      },
      {
        path: 'submissions/:id/resubmit',
        title: 'Revise assignment · Waypoint',
        loadComponent: () =>
          import('./features/youth/resubmit/youth-resubmit').then(
            (module) => module.YouthResubmitPage,
          ),
      },
      {
        path: 'submissions/:id',
        title: 'Submission status · Waypoint',
        loadComponent: () =>
          import('./features/youth/submission-status/youth-submission-status').then(
            (module) => module.YouthSubmissionStatusPage,
          ),
      },
      {
        path: 'announcements',
        title: 'Announcements · Waypoint',
        loadComponent: () =>
          import('./features/youth/announcements/youth-announcements').then(
            (m) => m.YouthAnnouncements,
          ),
      },
      {
        path: 'notifications',
        title: 'Notifications · Waypoint',
        loadComponent: () =>
          import('./features/youth/notifications/youth-notifications').then(
            (m) => m.YouthNotifications,
          ),
      },
    ],
  },
  {
    path: 'mentor',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'home', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Mentor dashboard · Waypoint',
        loadComponent: () =>
          import('./features/mentor/dashboard/mentor-dashboard').then(
            (module) => module.MentorDashboardPage,
          ),
      },
      {
        path: 'my-youth',
        title: 'My youth · Waypoint',
        loadComponent: () =>
          import('./features/mentor/roster/mentor-roster').then(
            (module) => module.MentorRosterPage,
          ),
      },
      {
        path: 'invitations',
        title: 'Invitations · Waypoint',
        loadComponent: () =>
          import('./features/shared/invitations/invitations').then(
            (module) => module.InvitationsPage,
          ),
      },
      {
        path: 'check-ins',
        title: 'Check-ins · Waypoint',
        loadComponent: () =>
          import('./features/mentor/check-ins/mentor-check-ins').then(
            (module) => module.MentorCheckInsPage,
          ),
      },
      {
        path: 'flags',
        title: 'At-risk youth · Waypoint',
        loadComponent: () =>
          import('./features/mentor/flags/mentor-flags').then((module) => module.MentorFlagsPage),
      },
      {
        path: 'youth/:id',
        title: 'Youth detail · Waypoint',
        loadComponent: () =>
          import('./features/mentor/youth-detail/mentor-youth-detail').then(
            (module) => module.MentorYouthDetailPage,
          ),
      },
      {
        path: 'assign-work',
        title: 'Assign work · Waypoint',
        loadComponent: () =>
          import('./features/mentor/assign-work/mentor-assign-work').then(
            (module) => module.MentorAssignWorkPage,
          ),
      },
      {
        path: 'calendar',
        title: 'Calendar · Waypoint',
        loadComponent: () =>
          import('./features/mentor/calendar/mentor-calendar').then(
            (module) => module.MentorCalendarPage,
          ),
      },
      {
        path: 'attendance',
        title: 'Attendance · Waypoint',
        loadComponent: () =>
          import('./features/mentor/attendance-history/mentor-attendance-history').then(
            (module) => module.MentorAttendanceHistoryPage,
          ),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/mentor/attendance-capture/mentor-attendance-capture').then(
            (module) => module.MentorAttendanceCapturePage,
          ),
      },
      {
        path: 'youth/:id/competencies',
        title: 'Competency tracker · Waypoint',
        loadComponent: () =>
          import('./features/mentor/competencies/mentor-competencies').then(
            (module) => module.MentorCompetenciesPage,
          ),
      },
      {
        path: 'reviews',
        title: 'Reviews · Waypoint',
        loadComponent: () =>
          import('./features/mentor/reviews/mentor-reviews').then(
            (module) => module.MentorReviewsPage,
          ),
      },
      {
        path: 'reviews/submissions/:id',
        title: 'Review submission · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/mentor/submission-review/mentor-submission-review').then(
            (module) => module.MentorSubmissionReviewPage,
          ),
      },
      {
        path: 'reviews/quizzes/:id',
        title: 'Review quiz assessment · Waypoint',
        loadComponent: () =>
          import('./features/mentor/quiz-review/mentor-quiz-review').then(
            (module) => module.MentorQuizReviewPage,
          ),
      },
      {
        path: 'messages',
        title: 'Messages · Waypoint',
        loadComponent: () =>
          import('./features/mentor/messages/mentor-messages').then(
            (module) => module.MentorMessagesPage,
          ),
      },
      {
        path: 'notifications',
        title: 'Notifications · Waypoint',
        loadComponent: () =>
          import('./features/youth/notifications/youth-notifications').then(
            (module) => module.YouthNotifications,
          ),
      },
    ],
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'home', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Director dashboard · Waypoint',
        loadComponent: () =>
          import('./features/admin/dashboard/admin-dashboard').then(
            (module) => module.AdminDashboardPage,
          ),
      },
      {
        path: 'people',
        title: 'People · Waypoint',
        loadComponent: () =>
          import('./features/admin/people/people-directory').then((m) => m.PeopleDirectory),
      },
      {
        path: 'people/:id',
        title: 'Person record · Waypoint',
        loadComponent: () =>
          import('./features/admin/people/person-detail').then((module) => module.PersonDetailPage),
      },
      {
        path: 'applications',
        title: 'Applications · Waypoint',
        loadComponent: () =>
          import('./features/admin/applications/applications-list').then((m) => m.ApplicationsList),
      },
      {
        path: 'invitations',
        title: 'Invitations · Waypoint',
        loadComponent: () =>
          import('./features/shared/invitations/invitations').then(
            (module) => module.InvitationsPage,
          ),
      },
      {
        path: 'users-roles',
        title: 'Users & roles · Waypoint',
        loadComponent: () =>
          import('./features/admin/users-roles/users-roles').then(
            (module) => module.UsersRolesPage,
          ),
      },
      {
        path: 'cohorts',
        title: 'Cohorts · Waypoint',
        loadComponent: () =>
          import('./features/admin/cohorts/cohorts-list').then((module) => module.CohortsListPage),
      },
      {
        path: 'cohorts/:id',
        title: 'Cohort detail · Waypoint',
        loadComponent: () =>
          import('./features/admin/cohorts/cohort-detail').then(
            (module) => module.CohortDetailPage,
          ),
      },
      {
        path: 'cohort-progress',
        title: 'Progress board · Waypoint',
        loadComponent: () =>
          import('./features/admin/progress/progress-board').then(
            (module) => module.ProgressBoardPage,
          ),
      },
      {
        path: 'reports',
        title: 'Reports hub · Waypoint',
        loadComponent: () =>
          import('./features/admin/reports/admin-reports').then(
            (module) => module.AdminReportsPage,
          ),
      },
      {
        path: 'import',
        title: 'Import roster · Waypoint',
        loadComponent: () =>
          import('./features/admin/import-roster/import-roster').then(
            (module) => module.ImportRosterPage,
          ),
      },
      {
        path: 'audit',
        title: 'Audit log · Waypoint',
        loadComponent: () =>
          import('./features/admin/audit/admin-audit-log').then(
            (module) => module.AdminAuditLogPage,
          ),
      },
      {
        path: 'privacy',
        title: 'Privacy requests · Waypoint',
        loadComponent: () =>
          import('./features/admin/privacy/admin-privacy').then(
            (module) => module.AdminPrivacyPage,
          ),
      },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'announcements' },
      {
        path: 'mentor-assignments',
        title: 'Mentor assignments · Waypoint',
        loadComponent: () =>
          import('./features/admin/mentors/mentor-assignments').then(
            (module) => module.MentorAssignmentsPage,
          ),
      },
      {
        path: 'tracks',
        title: 'Curriculum builder · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/admin/curriculum/curriculum-builder').then(
            (module) => module.CurriculumBuilderPage,
          ),
      },
      {
        path: 'badges',
        title: 'Badge builder · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/admin/curriculum/badge-builder').then(
            (module) => module.BadgeBuilderPage,
          ),
      },
      {
        path: 'progression',
        title: 'Progression framework · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/admin/curriculum/progression-builder').then(
            (module) => module.ProgressionBuilderPage,
          ),
      },
      {
        path: 'calendar',
        title: 'Program calendar · Waypoint',
        loadComponent: () =>
          import('./features/mentor/calendar/mentor-calendar').then(
            (module) => module.MentorCalendarPage,
          ),
      },
      {
        path: 'announcements',
        title: 'Announcement administration · Waypoint',
        loadComponent: () =>
          import('./features/admin/announcements/admin-announcements').then(
            (module) => module.AdminAnnouncementsPage,
          ),
      },
      {
        path: 'attendance',
        title: 'Attendance administration · Waypoint',
        loadComponent: () =>
          import('./features/mentor/attendance-history/mentor-attendance-history').then(
            (module) => module.MentorAttendanceHistoryPage,
          ),
      },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/mentor/attendance-capture/mentor-attendance-capture').then(
            (module) => module.MentorAttendanceCapturePage,
          ),
      },
      {
        path: 'settings',
        title: 'Organization settings · Waypoint',
        loadComponent: () =>
          import('./features/admin/settings/organization-settings').then(
            (module) => module.OrganizationSettingsPage,
          ),
      },
    ],
  },
  {
    path: 'volunteer',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Today · Waypoint',
        loadComponent: () =>
          import('./features/volunteer/today/volunteer-today').then((m) => m.VolunteerToday),
      },
      {
        path: 'schedule',
        title: 'Schedule · Waypoint',
        loadComponent: () =>
          import('./features/volunteer/schedule/volunteer-schedule').then(
            (m) => m.VolunteerSchedule,
          ),
      },
      { path: 'attendance', pathMatch: 'full', redirectTo: 'home' },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'attendance/:sessionId',
        title: 'Take attendance · Waypoint',
        canDeactivate: [unsavedChangesGuard],
        loadComponent: () =>
          import('./features/volunteer/attendance/volunteer-attendance').then(
            (module) => module.VolunteerAttendance,
          ),
      },
    ],
  },
  {
    path: 'guardian',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      {
        path: 'home',
        title: 'Guardian home · Waypoint',
        loadComponent: () =>
          import('./features/guardian/home/guardian-home').then((module) => module.GuardianHome),
      },
      {
        path: 'progress',
        title: 'Child progress · Waypoint',
        loadComponent: () =>
          import('./features/guardian/progress/guardian-progress').then(
            (module) => module.GuardianProgress,
          ),
      },
      {
        path: 'announcements',
        title: 'Guardian news · Waypoint',
        loadComponent: () =>
          import('./features/guardian/announcements/guardian-announcements').then(
            (module) => module.GuardianAnnouncements,
          ),
      },
      {
        path: 'consent',
        title: 'Consent and contact · Waypoint',
        loadComponent: () =>
          import('./features/guardian/consent/guardian-consent').then(
            (module) => module.GuardianConsent,
          ),
      },
      { path: 'notifications', pathMatch: 'full', redirectTo: 'announcements' },
    ],
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'profile' },
      {
        path: 'profile',
        title: 'Profile · Waypoint',
        loadComponent: () =>
          import('./settings/profile/settings-profile').then((m) => m.SettingsProfile),
      },
      {
        path: 'notifications',
        title: 'Notification settings · Waypoint',
        loadComponent: () =>
          import('./settings/notifications/settings-notifications').then(
            (m) => m.SettingsNotifications,
          ),
      },
      {
        path: 'display',
        title: 'Display & accessibility · Waypoint',
        loadComponent: () =>
          import('./settings/display/settings-display').then((m) => m.SettingsDisplay),
      },
      {
        path: 'privacy',
        title: 'Privacy & data rights · Waypoint',
        loadComponent: () =>
          import('./settings/privacy/settings-privacy').then(
            (module) => module.SettingsPrivacyPage,
          ),
      },
    ],
  },
  {
    path: 'help',
    title: 'Help center · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('./help/help').then((module) => module.HelpPage),
  },
  {
    path: 'support',
    title: 'Contact support · Waypoint',
    canActivate: [authGuard],
    loadComponent: () => import('./support/support').then((m) => m.Support),
  },
  {
    path: 'consent-required',
    title: 'Consent needed · Waypoint',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./states/consent-required/consent-required').then((m) => m.ConsentRequired),
  },
  {
    path: 'session-expired',
    title: 'Session expired · Waypoint',
    loadComponent: () =>
      import('./states/session-expired/session-expired').then((m) => m.SessionExpired),
  },
  {
    path: 'forbidden',
    title: 'No access · Waypoint',
    loadComponent: () => import('./states/forbidden/forbidden').then((m) => m.Forbidden),
  },
  {
    path: 'server-error',
    title: 'Something went wrong · Waypoint',
    loadComponent: () => import('./states/server-error/server-error').then((m) => m.ServerError),
  },
  {
    path: 'maintenance',
    title: 'Maintenance · Waypoint',
    loadComponent: () => import('./states/maintenance/maintenance').then((m) => m.Maintenance),
  },
  {
    path: 'offline',
    title: 'Offline · Waypoint',
    loadComponent: () => import('./states/offline/offline-state').then((m) => m.OfflineState),
  },
  {
    path: '**',
    title: 'Not found · Waypoint',
    loadComponent: () => import('./states/not-found/not-found').then((m) => m.NotFound),
  },
];
